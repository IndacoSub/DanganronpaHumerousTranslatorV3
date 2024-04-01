# DanganronpaHumerousTranslatorV3

# This is a fork of [DanganronpaHumerousTranslator](https://github.com/morgana-x/DanganronpaHumerousTranslator) focused on DRV3 (thanks @morgana-x !)

# DR1, DR2, DRAE and DRS aren't supported

A modified version of [Danganronpa Tools](https://github.com/vn-tools/danganronpa-tools) to Google Translate all the text in the game's scripts 10 times for funny results

Original project / Inspiration: https://gamebanana.com/mods/50041

This can also be modified to do all sorts of large scale text processing.

This program has incredible speed due to the extensive use of parallel processing. 

(So fast that the Google Translate end point cops it every now and again and says there are too many requests, oops.)

You can edit the amount of threads or just switch to another network and re-run the program to continue if that happens.

# How to use

Change your directory in the command prompt to the location of the executable and type:

```cs
danganronpafunnytranslator -drv3 in_folder out_folder
```

The "in_folder" should contain .txt files extracted with [SPCTool and STXTool](https://github.com/CaptainSwag101/DRV3-Sharp).
