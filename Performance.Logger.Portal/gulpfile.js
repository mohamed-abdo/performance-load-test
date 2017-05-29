/// <binding Clean='install-scripts' />

var gulp = require('gulp'),
    install = require("gulp-install");


gulp.task('install-scripts', function () {
    return gulp.src(['./package.json'], { read: false, production: true, ignoreScripts: true, noOptional: true })
      .pipe(install({
          ignoreScripts: true, interactive: false, forceLatest: true, production: true
      }));
});

gulp.task('default', ['install-scripts'], function (cb) {
    console.log(cb);
    // place code for your default task here
});