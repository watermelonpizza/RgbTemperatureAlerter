# RGB Temperature Alerter
Flashes corsair and logitech rgbs when computer is over specified temperature thresholds 

Used with HWiNFO alert settings

![hwinfoscreenshot][]

[hwinfoscreenshot]: https://raw.githubusercontent.com/watermelonpizza/rgbtempalerter/master/assets/HWiNFO64_screenshot.png?sanitize=true

## Usage
Runs once silently then exits, locks a file so only one can run at the same time

| Command                                 | Description           | Default Value  |
| ----------------------------------------| ----------------------| ---------------|
| `-t <value>` or `--temperature <value>` | the temperature       | **required**   |
| `-w <value>` or `--warn <value>`        | temp to warn at       | 75             |
| `-d <value>` or `--danger <value>`      | temp to alert at      | 80             |
| `-i <seconds>` or `--interval <seconds>`| how fast to flash     | .5             |
| `-s <seconds>` or `--seconds <seconds>` | how long to flash for | 2              |