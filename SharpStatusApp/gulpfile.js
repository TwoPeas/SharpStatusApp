const postcss = require('gulp-postcss'),
    tailwindcss = require('tailwindcss'),
    del = require('del'),
    gulp = require('gulp'),
    concat = require('gulp-concat'),
    browserSync = require('browser-sync').create();

function clean() {
    return del(['./wwwroot/**', '!./wwwroot'], { force: true });
}

function copyMisc() {
    return gulp.src('./public/*.*')
        .pipe(gulp.dest('./wwwroot'));
}

function copyImages() {
    return gulp.src('./public/img/**/*.*')
        .pipe(gulp.dest('./wwwroot/img'));
}

function buildStyles() {
    return gulp.src('./public/css/*.css')
        .pipe(concat('site.css'))
        .pipe(postcss([
            tailwindcss(),
            require('postcss-nested')({
                bubble: ['screen'],
            }),
            require('autoprefixer'),
        ]))
        .pipe(gulp.dest('./wwwroot/css'))
        .pipe(browserSync.stream());
}

function watchFiles(watchCb) {
    function _logWatch(callerName, type, path) {
        console.log(`======> ${new Date().toLocaleTimeString()} - ${callerName}: ${path} - ${type}`);
    }

    const assetWatcher = gulp.watch(['./public/img/*.*'], gulp.series(copyImages));
    assetWatcher.on('all',
        function (type, path) {
            _logWatch('Images', type, path);
        });

    const styleWatcher = gulp.watch(['./public/css/*.css', './tailwind.config.js'], gulp.series(buildStyles));
    styleWatcher.on('all',
        function (type, path) {
            _logWatch('Style', type, path);
        });

    const pageWatcher = gulp.watch(['./Pages/**/*.*']);
    pageWatcher.on('all',
        function (type, path) {
            _logWatch('Page', type, path);
            browserSync.reload();
        }
    );

    watchCb();
}

gulp.task('clean', clean);

gulp.task('default',
    gulp.series(
        clean,
        gulp.series(
            copyMisc,
            copyImages,
            buildStyles,
        )
    )
);

gulp.task('browser-sync', function () {
    browserSync.init({
        proxy: "localhost:5000"
    });
});

gulp.task('watch', gulp.series(
    'default',
    gulp.parallel(
        'browser-sync',
        watchFiles
    )
));
