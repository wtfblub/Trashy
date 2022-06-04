# Trashy
A [VTube Studio](https://denchisoft.com/) mod that lets your Twitch Viewers throw items at you.

![Preview](https://github.com/wtfblub/Trashy/raw/dev/media/preview.gif?)


## Installation

### Requirements
1. Windows
2. [BepInEx](https://github.com/BepInEx/BepInEx)

### Installing BepInEx
The official installation guide can be found on the [BepInEx website](https://docs.bepinex.dev/).

**Quick guide:**
Simply download the BepInEx 5 64bit package from [here](https://github.com/BepInEx/BepInEx/releases/download/v5.4.15/BepInEx_x64_5.4.15.0.zip) and extract the contents to the VTube Studio directory.

**To find the VTube Studio directory:**
1. Open Steam
2. Right click VTube Studio
3. Manage > Browse local files

![](https://github.com/wtfblub/Trashy/raw/dev/media/steam_browse_files.png)

### Installing Trashy
1. Download the latest release from [here](https://github.com/wtfblub/Trashy/releases)
2. Extract the contents to `VTube Studio\BepInEx\plugins`
    *Just create the plugins folder if its missing*

Open VTube Studio and go to the settings, you should find the new Trashy Icon on the top bar:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config_icon.png)

Clicking on it should open up the configuration window:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config.png?)

Click on `Connect with Twitch` it should open a tab in your browser where you can login and connect your Twitch account.

After logging in it should look like this:

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config_loggedin.png?)


## Settings

- *Item size:* The size for your items. All your png files will be scaled to this size. More on how items work further below

- *Model reaction:* If your model should react to items hitting

![](https://github.com/wtfblub/Trashy/raw/dev/media/preview.gif?)

- *Reaction power:* How strong your model will react

- *Play hit sound:* If enabled, a sound effect will be played when items hit your model

### Setting up triggers

Triggers are conditions when to throw items. Click on `Edit Triggers` to configure them(or click it again to close the Triggers window).

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config_triggers.png)

Click on `Add trigger` and choose the type of trigger you want.

- **Redeem:** A channel points redeem
  * Go into your Twitch Dashboard and create a `Channel points reward` then put the exact same name for the Reward into the `Redeem Name` field in the Trashy settings
  
  ![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_config_trigger_redeem.png)

- **Bits:** A bits donation
  * *Minimum Bits:* Only triggers if this amount of bits was donated. You can use this to prevent 1 bit donation spam

- **Sub:** A subscription or re-subscription

- **GiftSub:** A gifted subscription or community gifted subscriptions
  * *Note:* This will get triggered for each community gifted subscription. Example: If someone gifts 100 subscriptions it will trigger 100 times

- **Command:** A chat command
  * *Command name:* The chat command, defaults to `!throw`
  * *Cooldown:* How many seconds need to pass before the command can be used again(Broadcaster has no cooldown)
  * *Command restriction:* Who is allowed to use the command. Subscriber: subs, vips, mods - Vip: vips, mods etc 

**Other trigger settings:**
- *Enabled:* If the trigger is active or not

- *Item Count:* The number of items to throw each time this trigger happens

- *Sticky chance:* The chance how likely items are going to stick to your model
  
  ![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_sticky_preview.gif)

- *Sticky duration:* How long a item will stick to your model

- *Item Group:* The item group to use. This will get explained further below

- *Test button:* Tests the trigger

### Setting up items

Trashy loads items as png files from the `VTube Studio\BepInEx\plugins\Trashy\Items` folder. Simply drop any png file into this folder and press `Reload Items` in the Trashy config.

![](https://github.com/wtfblub/Trashy/raw/dev/media/trashy_items.png)

The preferred size for png files is at least 256x256 but any size is fine because Trashy will scale the image accordingly to your `Item Size` setting.

**Item groups**

Groups are made by creating folders in `VTube Studio\BepInEx\plugins\Trashy\Items`.

Example: Create a folder `pokemon` in `VTube Studio\BepInEx\plugins\Trashy\Items` and drop some png files into the `pokemon` folder. Now you have the group `pokemon` available in the trigger settings and it will only throw items from the `pokemon` folder.

Make sure to click on `Reload Items` in the settings after adding or changing png files.

### Additional hit sounds

Trashy comes with a default hit sound, but you can replace it and/or add other sounds by just adding files to the `VTube Studio\BepInEx\plugins\Trashy\Sounds` folder, provided they are in one of these supported formats:

 - `.mp3` MPEG Layer 3
 - `.ogg` Ogg Vorbis (not Opus!)
 - `.wav` Microsoft Wave
 - `.aiff` Audio Interchange File Format

Which sound is played when hit is chosen at random.

If you add/remove sounds while VTube Studio is open, remember to click `Reload hit sounds` to apply the changes.