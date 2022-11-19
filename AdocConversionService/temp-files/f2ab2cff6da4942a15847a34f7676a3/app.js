const fs = require('fs');
const nodeHtmlToImage = require('node-html-to-image');

fs.readFile('F:\workspace\csharp\AdocConversionService\AdocConversionService\AdocConversionService\temp-files\f2ab2cff6da4942a15847a34f7676a3\foo.html', 'utf8', function(err, data) {
    if (err) throw err;
    console.log('OK');
    nodeHtmlToImage({
        output: 'F:\workspace\csharp\AdocConversionService\AdocConversionService\AdocConversionService\temp-files\f2ab2cff6da4942a15847a34f7676a3\image.jpeg',
        html: data,
        type: 'jpeg',
        puppeteerArgs: 
        {
            args: ['--no-sandbox']
        }
    })
    .then(() => console.log('The image was created successfully!'));
});