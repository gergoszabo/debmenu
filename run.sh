#!/usr/bin/env bash
# Endless loop: run ./debmenu when current time matches cron expression 0,30 8-11 * * *
# i.e. at minute 0 and 30 of hours 8, 9, 10, 11 (every day)

while true; do
    now=$(date +%H:%M)
    hour=${now%%:*}
    minute=${now##*:}

    # Strip leading zero for arithmetic comparison
    hour=$((10#$hour))
    minute=$((10#$minute))

    # Check cron match: minutes 0,30 AND hours 8-11
    if (( (minute == 0 || minute == 30) && hour >= 8 && hour <= 11 )); then
        ./debmenu
    fi

    sleep 30
done
