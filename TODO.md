# TODO
Herein lies a list of enhancements that need to be made before release

### Sources
* Refactor sources from a list of hardcoded strings to something less amateur
* Add WowInterface
* Add Git (Github and Gitlab)

### Configuration
* Move config files to %APPDATA%
* Check for WoW path on first run, prompt if it can't find WoW, then generate `config.ini`

### Data
* Add ability to remove Addons from list
* Implement version checking for Addons
  * Version checking for each source
  * Only update addons if there is a new version

### UI
* Make it less ugly
* Make the Add popup more intuitive
* Refactor workflow so it actually makes sense

### Deployment
* Write bootstrap script for setting up dev environment
* Build installer
* Write documentation - install
* Write documentation - usage
