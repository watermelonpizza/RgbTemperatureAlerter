# RGB Temperature Alerter
Flashes corsair and logitech rgbs when computer is over specified temperature thresholds.

Warn flashes yellow ![yellow][] (#FFFF00)

Danger flashes red ![red][] (#FF0000)

[yellow]: https://via.placeholder.com/15/ffff00/000000?text=%20
[red]: https://via.placeholder.com/15/ff0000/000000?text=%20

Used with HWiNFO alert settings

![hwinfoscreenshot][]

[hwinfoscreenshot]: https://raw.githubusercontent.com/watermelonpizza/RgbTemperatureAlerter/master/assets/HWiNFO64_screenshot.png

## Usage
Runs once silently then exits, locks a file so only one can run at the same time

| Command                                 | Description           | Default Value  |
| ----------------------------------------| ----------------------| ---------------|
| `-t <value>` or `--temperature <value>` | the temperature       | **required**   |
| `-w <value>` or `--warn <value>`        | temp to warn at       | 75             |
| `-d <value>` or `--danger <value>`      | temp to alert at      | 80             |
| `-i <seconds>` or `--interval <seconds>`| how fast to flash     | .5             |
| `-s <seconds>` or `--seconds <seconds>` | how long to flash for | 2              |
