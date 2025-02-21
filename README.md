# pdf2eink

convert pdf/djvu -> .cb (customized book)

## Custom book format
CB is a simple WYSIWYG image based format for e-books

| Offset    |Size (bytes)| Description |
| -------- |--| ------- |
| 0x00 | 3|'CB' Signature    |
| 0x03 | 1|Format type|
| 0x04 | 4|Pages qty     |
| 0x08  |2  | Width    |
| 0x0A  |2  | Height   |
| 0x0C  |variable. Pages x Width x Height / 8  | Monochrome pages image data   |


You can read CB book using TurtleBook (https://github.com/fel88/TurtleBook)


### References

Dithering code from here: https://github.com/cyotek/Dithering
