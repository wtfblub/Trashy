# Trashy
A [VTube Studio](https://denchisoft.com/) mod that lets your Twitch Viewers throw items at you.

![Preview](https://github.com/wtfblub/Trashy/raw/dev/media/preview.gif)


## Installation

### Requirements
1. Windows
2. [BepInEx](https://github.com/BepInEx/BepInEx)

### Installing BepInEx
The official installation guide can be found on the [BepInEx website](https://docs.bepinex.dev/).

**Quick guide:**
Simply download the BepInEx 5 64bit package from [here](https://github.com/BepInEx/BepInEx/releases/download/v5.4.15/BepInEx_x64_5.4.15.0.zip) and extract the contents to the VTube Studio directory.

To find the VTube Studio directory:
1. Open Steam
2. Right click VTube Studio
3. Manage > Browse local files

![](https://github.com/wtfblub/Trashy/raw/dev/media/steam_browse_files.png)

### Installing Trashy
1. Download the latest release from [here](https://github.com/wtfblub/Trashy/releases)
2. Extract the contents to `VTube Studio\BepInEx\plugins`
  * Just create the plugins folder if its missing

Open VTube Studio and go to the settings, you should find the new Trashy Icon on the top bar:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config_icon.png)

Clicking on it should open up the configuration window:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config.png)

Click on `Connect with Twitch` it should open a tab in your browser where you can login and connect your account.

After logging in it should look like this:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config_loggedin.png)

Now simply create a new Channel Points Reward in your Twitch Dashboard and name it however you like, then enter the name you used here in `Twitch Reward Name`. What is left now is adding items to Trashy.

Trashy loads items from `VTube Studio\BepInEx\plugins\Trashy\Items` in order to add items simply drop some png files in this folder like this:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_items.png)

The preferred size for png files is at least 256x256 but any size is fine since Trashy will scale the image accordingly to your `Sprite Size` setting.

Make sure to click on `Reload Items` in the settings after adding new png files.

The number of items thrown per Channel Points redeem can be set with `Object Count` in the settings.

You can press `Shift + Right Click` anywhere in VTube Studio to throw items and see how it looks without needing to redeem anything.