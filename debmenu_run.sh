#!/bin/bash

# Log datetime before running script
date '+%Y-%m-%d %H:%M:%S Running debmenu_run.sh' >> /apps/debmenu/log.log
# Execute the debmenu script and append output to log.log
node /apps/debmenu/debmenu.cjs >> /apps/debmenu/log.log 2>&1
