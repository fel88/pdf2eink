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

## Boustrophedon mode (mirror read)

You can also convert .cb book to the mirror mode 


This can save you about 10% of the reading time, because in this case you don't need to make any saccades with your eyes.


https://en.wikipedia.org/wiki/Saccade

https://en.wikipedia.org/wiki/Boustrophedon

![MSS-Boustrophedon-Example](https://github.com/user-attachments/assets/9d295786-539b-434d-a540-46af1d4e11c0)


### References

Dithering code from here: https://github.com/cyotek/Dithering
