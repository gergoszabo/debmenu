#!/bin/bash

cd <absolute-path>
sleed 30
while true; do
    cronitor exec <abcdef> ./env_variables.prod.sh
    sleep 14380
done