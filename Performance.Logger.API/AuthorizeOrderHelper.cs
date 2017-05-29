using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AgilePay.GeexGate.Api.Model.Payment.General;
using AgilePay.GeexGate.Api.Payment.Rest.Helper.Tokenization;
using AgilePay.GeexGate.Api.Payment.Rest.Models;
using AgilePay.GeexGate.DAL.PaymentStore.Abstract;
using AgilePay.GeexGate.DAL.PaymentStore.Concrete;
using AgilePay.GeexGate.DAL.PaymentStore.EntityModel;
using AgilePay.GeexGate.Api.Model.Payment;
using AgilePay.GeexGate.Api.Client.MPGS.Helper2.Tokenization;
using AgilePay.GeexGate.Api.Client.WhiteLabel.PG.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;

namespace AgilePay.GeexGate.Api.Payment.Rest.Helper.Order2
{
    public class AuthorizeOrderHelper : OrderHelper
    {
        const string format = "yyyy-MM-dd hh:mm:ss:fff";
        #region Process
        protected override async Task<GeneralResponseModel> processRequestAsync()
        {
            string orderId = Request.Order.Id;
            using (IPaymentRepository repo = new PaymentRepository())
            {
                var timer = Stopwatch.StartNew();
                var startTime = DateTime.Now;
                Order order = await repo.FindOrderByReferenceAsync(orderId);
                var ms = timer.ElapsedMilliseconds;
                Trace.WriteLine($"1- Ref -Order {order.Reference} Start {startTime.ToString(format)} End {DateTime.Now.ToString(format)} Duration {ms}");
                if (order == null)
                {
                    throw new PaymentException(PaymentErrorCodes.InputInvalid, new Dictionary<string, string>() { { "fieldName", "orderId" } });
                }
                FillReferenceData(repo);

                if (!order.ActiveOrderStatu.OrderStatusType.Name.Equals(Models.Constant.ORDER_STATUS_INITIATED))
                {
                    throw new PaymentException(PaymentErrorCodes.OrderOperationNotAllowed,
                        new Dictionary<string, string>() { { "orderState", order.ActiveOrderStatu.OrderStatusType.Name } });
                    //throw new HttpResponseException(Util.FactoryHttpResponse(Constant.ERROR_ORDER_ALREADY_AUTHORIZED, String.Format(Constant.ERROR_DESC_ORDER_ALREADY_AUTHORIZED, order.ActiveOrderStatu.OrderStatusType.Name), Constant.ERROR_ORDER_ALREADY_AUTHORIZED, System.Net.HttpStatusCode.Forbidden));
                }

                var cardEnrolled = _3DSecureStatusTypeMapperList.FirstOrDefault(d => d.Name.Equals("CARD_ENROLLED"));
                var authenticationSuccessful = _3DSecureStatusTypeMapperList.FirstOrDefault(d => d.Name.Equals("AUTHENTICATION_SUCCESSFUL"));
                var cardDoesNotSupport3Ds = _3DSecureStatusTypeMapperList.FirstOrDefault(d => d.Name.Equals("CARD_DOES_NOT_SUPPORT_3DS"));
                var cardNotEnrolled = _3DSecureStatusTypeMapperList.FirstOrDefault(d => d.Name.Equals("CARD_NOT_ENROLLED"));

                if (order.OrderConfiguration.Check3DSecure != null && (bool)order.OrderConfiguration.Check3DSecure)
                {
                    if (order.Order3DSecureEnroll == null)
                    {
                        throw new PaymentException(PaymentErrorCodes.Order3DSecureCheckFailed);
                        // throw error 3D Check not prerformed
                    }
                    if (order.Order3DSecureEnroll.ActiveOrder3DSecureEnrollStatus.C3DSecureStatusTypeMapper.C3DSecureStatusType.Name == cardNotEnrolled?.Name )//||
                        //order.Order3DSecureEnroll.ActiveOrder3DSecureEnrollStatus.C3DSecureStatusTypeMapper.C3DSecureStatusType.Name == cardDoesNotSupport3Ds?.Name)
                    {
                        throw new PaymentException(PaymentErrorCodes.Order3DSecureCheckFailed);
                        // throw error only if merchant disallow not 3D Secure transaction
                    }
                    if (
                        order.Order3DSecureEnroll.ActiveOrder3DSecureEnrollStatus.C3DSecureStatusTypeMapper.C3DSecureStatusType.Name == cardEnrolled?.Name &&
                        order.Order3DSecureEnroll.ActiveOrder3DSecureEnrollStatus.C3DSecureGatewayCodeTypeMapper.C3DSecureGatewayCodeType.Name == authenticationSuccessful?.Name)
                    {
                        // card enrolled and successfully authenticated
                    }
                }

                Request.Transaction = new TransactionModel()
                {
                    Id = Guid.NewGuid().ToString()
                };

                timer = Stopwatch.StartNew();
                startTime = DateTime.Now;
                UpdateRequest(Request, order);
                ms = timer.ElapsedMilliseconds;
                Trace.WriteLine($"2- UpdateReq- Order {order.Reference} Start {startTime.ToString(format)} End {DateTime.Now.ToString(format)} Duration {ms}");

               

                if (string.IsNullOrEmpty(Request.Session?.Id))
                {
                    if (string.IsNullOrEmpty(Request.SourceOfFunds?.Token2?.Identifier) && (string.IsNullOrEmpty(Request.SourceOfFunds.Provided?.Card?.Number)))
                    {
                        throw new PaymentException(PaymentErrorCodes.OrderPaymentInfoMissing);
                    }
                }
                else
                {
                    if (Request.SourceOfFunds == null)
                        Request.SourceOfFunds = new SourceOfFundsModel();
                    if (string.IsNullOrEmpty(Request.SourceOfFunds.Type))
                        Request.SourceOfFunds.Type = "CARD";
                }


                string merchantId = order.MerchantAccount.MerchantIdentifier;
                string authParam = order.MerchantAccount.ActiveMerchantAccountApiKey.PrivateKey;
                //var helper = FactoryAdapter.GetHelper<GeneralRequestModel, GeneralResponseModel>(ConstantWhiteLabelPG.MPGS, ConstantApiOperation.Authorize);//= new Client.MPGS.Helper2.Transaction.AuthorizeOrderHelper();
                //Response = await helper.ProcessAsync(Request, merchantId, authParam);
                var helper = FactoryAdapter.GetHelper<GeneralRequestModel, GeneralResponseModel>(order.MerchantAccount.PaymentGatewayType.Name, ConstantApiOperation.Authorize);//new Check3DEnrollHelper();

                timer = Stopwatch.StartNew();
                startTime = DateTime.Now;
                Response = await helper.ProcessAsync(Request, merchantId, authParam);
                ms = timer.ElapsedMilliseconds;
                Trace.WriteLine($"3- Gateway- Order {order.Reference} Start {startTime.ToString(format)} End {DateTime.Now.ToString(format)} Duration {ms}");



                OrderTxn txn = null;
                if (Models.Constant.RESULT_SUCCESS.Equals(Response.Result))
                {

                    if (order.OrderConfiguration.TokenizeCc != null && (bool)order.OrderConfiguration.TokenizeCc && order.ActiveOrderFundsSrc?.MerchantTokenIdentifier == null )
                        //&& // TO DO: update it to save user consent
                        //order.OrderConfiguration.PayerConsentForToken!=null && (bool)order.OrderConfiguration.PayerConsentForToken)
                    {
                        if (order.ActiveOrderFundsSrc?.MerchantToken != null)
                        {
                            Response.SourceOfFunds.Token2 = new TokenModel()
                            {
                                Identifier = order.ActiveOrderFundsSrc.MerchantToken.Identifier
                            };
                        }
                        else
                        {
                            GeneralRequestModel tokenReqModel = new GeneralRequestModel()
                            {
                                Merchant = new MerchantModel()
                                {
                                    Id = order.Business.Organization.Identifier
                                },
                                Order = new OrderModel
                                {
                                    Id = order.Reference
                                },
                                SourceOfFunds = new SourceOfFundsModel {
                                    Type = "CARD",
                                    Token2 = new TokenModel()
                                    {
                                        Identifier = Guid.NewGuid().ToString()
                                    }
                                },                    
                                Session = new SessionModel {
                                    Id = order.OrderSessions.FirstOrDefault(s=>s.IsActive==true).Identifier
                                }
                            };
                           
 
                            //var h = FactoryAdapter.GetHelper<GeneralRequestModel, GeneralResponseModel>(ConstantWhiteLabelPG.MPGS, ConstantApiOperation.CreateToken);//= new CreateUpdateTokenHelper();
                            //GeneralResponseModel tokenResModel = await h.ProcessAsync(tokenReqModel, merchantId, authParam);
                            var h = FactoryAdapter.GetHelper<GeneralRequestModel, GeneralResponseModel>(order.MerchantAccount.PaymentGatewayType.Name, ConstantApiOperation.CreateToken);//new Check3DEnrollHelper();

                            GeneralResponseModel tokenResModel = await h.ProcessAsync(Request, merchantId, authParam);
                           if (Models.Constant.RESULT_SUCCESS.Equals(tokenResModel.Result))
                            {
                                
                                if (Response.Result.Equals(Models.Constant.RESULT_SUCCESS))
                                {

                                    //MerchantAccount merchantAccount = order.MerchantAccount;
                                    MerchantToken merchantToken = new MerchantToken
                                    {
                                        Business = order.MerchantAccount.Business,
                                        MerchantAccount = order.MerchantAccount,
                                        PgIdentifier = tokenResModel.SourceOfFunds.Token2.Identifier,
                                        Identifier = Guid.NewGuid().ToString(),
                                        CreatedOn = DateTime.UtcNow,
                                        IsActive = true
                                    };

                                    MerchantTokenCc merchantTokenCc = new MerchantTokenCc
                                    {
                                        Brand = Response.SourceOfFunds.Provided.Card.Brand,
                                        ExpiryMonth = Convert.ToInt32(Response.SourceOfFunds.Provided.Card.Expiry.Month),
                                        ExpiryYear = Convert.ToInt32(Response.SourceOfFunds.Provided.Card.Expiry.Year),
                                        MaskedNumber = Response.SourceOfFunds.Provided.Card.Number,
                                        Scheme = Response.SourceOfFunds.Provided.Card.Scheme,
                                        MerchantToken = merchantToken
                                    };

                                    merchantToken.MerchantTokenCc = merchantTokenCc;

                                    order.MerchantAccount.MerchantTokens.Add(merchantToken);
                                    Response.SourceOfFunds.Token2 = new TokenModel()
                                    {
                                        Identifier = merchantToken.Identifier
                                    };
                                }
                            }

                        }


                    }

                    txn = AddOrderTxn(Request, Response, order);
                    UpdateOrderBalance(order, (decimal)Response.Order.TotalAuthorizedAmount, (decimal)Response.Order.TotalCapturedAmount, (decimal)Response.Order.TotalRefundedAmount);
                   
                    if (Response.Device != null)
                    {
                        order.OrderDevice = new OrderDevice
                        {
                            Browser = Response.Device.Browser,
                            IpAddress = Response.Device.IpAddress
                        };
                    }

                    if (order.ActiveOrderFundsSrc == null)
                    {
                        OrderFundsSrc srcFunds = new OrderFundsSrc()
                        {
                            Type = Response.SourceOfFunds.Type,
                            CreatedOn = DateTime.UtcNow,
                            Order = order,
                            IsActive = true
                        };
                        if (Response.SourceOfFunds.Token2 != null && Response.SourceOfFunds.Token2.Identifier != null) {
                            srcFunds.MerchantToken = order.MerchantAccount.MerchantTokens.FirstOrDefault(t => t.Identifier.Equals(Response.SourceOfFunds.Token2.Identifier));
                            srcFunds.MerchantTokenIdentifier = Response.SourceOfFunds.Token2.Identifier;
                        }
                        order.OrderFundsSrcs.Add(srcFunds);
                    }
                    if (Response.SourceOfFunds?.Provided?.Card != null && order.ActiveOrderFundsSrc?.OrderCardDetail == null)
                    {
                        OrderCardDetail orderCardDetail = new OrderCardDetail()
                        {
                            Brand = Response.SourceOfFunds.Provided.Card.Brand,
                            ExpiryMonth = Convert.ToInt32(Response.SourceOfFunds.Provided.Card.Expiry.Month),
                            ExpiryYear = Convert.ToInt32(Response.SourceOfFunds.Provided.Card.Expiry.Year),
                            CardHolderName = Response.SourceOfFunds.Provided.Card.NameOnCard,
                            MaskedNumber = Response.SourceOfFunds.Provided.Card.Number,
                            Scheme = Response.SourceOfFunds.Provided.Card.Scheme,
                            FundingMethod = Response.SourceOfFunds.Provided.Card.FundingMethod
                        };
                        order.ActiveOrderFundsSrc.OrderCardDetail = orderCardDetail;
                        order.ActiveOrderFundsSrc.PayerInfo = orderCardDetail.MaskedNumber;
                        order.ActiveOrderFundsSrc.PaymentInfo = orderCardDetail.Scheme;
                        order.ActiveOrderFundsSrc.Type = "CARD";

                        if (Response.SourceOfFunds.Provided.Card.Response?.Cvv != null)
                        {
                            OrderTxnRespAcqDetail d = new OrderTxnRespAcqDetail() {
                                AcqCode = Response.SourceOfFunds.Provided.Card.Response.Cvv.AcquirerCode,
                                GatewayCode = Response.SourceOfFunds.Provided.Card.Response.Cvv.GatewayCode,
                                Type = "CardSecurityCode"
                            };

                            txn.OrderTxnRespAcq.OrderTxnRespAcqDetails.Add(d);
                        }
                    }

                    if (order.ActiveOrderFundsSrc != null && order.ActiveOrderFundsSrc.MerchantToken != null) {
                        if (Response.SourceOfFunds == null)
                            Response.SourceOfFunds = new SourceOfFundsModel();
                        if (Response.SourceOfFunds.Token2 == null)
                            Response.SourceOfFunds.Token2 = new TokenModel();
                        Response.SourceOfFunds.Token2.Identifier = order.ActiveOrderFundsSrc.MerchantToken.Identifier;
                    }

                    UpdateOrderStatus(order, Response);
                }
                else
                {
                }
                timer = Stopwatch.StartNew();
                startTime = DateTime.Now;
                AddEvent(Request, Response, order, txn);
                ms = timer.ElapsedMilliseconds;
                Trace.WriteLine($"4- Add Event- Order {order.Reference} Start {startTime.ToString(format)} End {DateTime.Now.ToString(format)} Duration {ms}");
              
                timer = Stopwatch.StartNew();
                startTime = DateTime.Now;
                await repo.SaveOrderAsync(order);
                ms = timer.ElapsedMilliseconds;
                Trace.WriteLine($"5- Save- Order {order.Reference} Start {startTime.ToString(format)} End {DateTime.Now.ToString(format)} Duration {ms}");

                         }




            return Response;

        }



        #endregion

        #region Validate
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        protected override bool validateRequest()
        {
            if (Request.Order?.Id == null)
            {
                throw new PaymentException(PaymentErrorCodes.InputInvalid, new Dictionary<string, string>() { { "fieldName", "orderId" } });
            }

            return true;

        }
        #endregion

    }
}