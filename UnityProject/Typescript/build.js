const glob = require('glob');
const rimraf = require('rimraf');
const fs = require('fs');

glob.sync(`${__dirname}/../Assets/Javascript/**/*.js`)
    .forEach(filepath => {
        fs.writeFileSync(
            filepath + ".txt",
            fs.readFileSync(filepath, 'utf-8')
        )
        rimraf.sync(filepath)
    });
