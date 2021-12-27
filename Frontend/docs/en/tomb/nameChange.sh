#!/bin/sh
MYLEN=0
NUM=0
for filename in ./*.html; do
    MYLEN=${#filename};
    NUM=${filename:2:2};
    UNDERSCORE=${filename:3:1};
    #if [[ $UNDERSCORE != *"_"* ]]; then 
    #    NUMPLUS=$((NUM+1));
    #    NAME=$NUMPLUS${filename:4:MYLEN};
    #    echo "${filename} _ ${NAME}";
    #    $(mv $filename $NAME);
    #fi;
    #if [[ $UNDERSCORE != *"_"* ]]; then
    #    if [[ $NUM -gt 33 ]]; then
    #        NUMPLUS=$((NUM+1));
    #        NAME=$NUMPLUS${filename:4:MYLEN};
    #        echo "${filename} _ ${NAME}";
    #        $(mv $filename $NAME);
    #    fi;
    #fi;
    if [[ $UNDERSCORE != *"_"* ]]; then
        NAME=${filename:2:2}${filename:9:MYLEN};
        #echo "$NAME";
        $(mv $filename $NAME);
    fi;
done