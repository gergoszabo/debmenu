#!/bin/bash
# Setup cron for debmenu_run.sh at 8:00 and 11:00 weekdays
CRON_ENTRY="0 8,11 * * 1-5 /apps/debmenu/gemini/debmenu_run.sh"
CRONTAB=$(crontab -l 2>/dev/null)
if ! echo "$CRONTAB" | grep -Fxq "$CRON_ENTRY"; then
    (echo "$CRONTAB"; echo "$CRON_ENTRY") | crontab -
    echo "Crontab updated: debmenu_run.sh will be executed at 8:00 and 11:00 every weekday."
else
    echo "Crontab entry already exists. No changes made."
fi
