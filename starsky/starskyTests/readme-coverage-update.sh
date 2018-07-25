#!/bin/bash
# DATE=`date '+%Y-%m-%d'` #  %H:%M:%S
#
# cd "$(dirname "$0")"
#
# touch readme.bak
#
# ENABLEDELETE=false
# cat readme.md | while read line
# do
# 	if [ "$line" = "### Coverage Chart" ]; then
# 	  echo "x has the value 'valid'"
# 	fi
#
# 	# if[[ $ENABLEDELETE = "true"]] then
# 	# 	echo $line
# 	# fi
#     # do something with $line here
# done
#
#
#
#
#
# # grep -E -o "\+---.+\n\|.+\n\+----------.+\n\|.+\n\+---[-\+]+\n\|+.+\n\+---[-\+]+" readme.coverage.bak
# # \+---.+\n\|.+\n\+----------.+\n\|.+\n\+---[-\+]+\n\|+.+\n\+---[-\+]+
#
# # grep -E -o "(\+-.+)|\| [MALBa-z 0-9,%|]+" readme.coverage.bak
#
# sed -e 's/^\+-*+-*+-*+-*+/''/g' -e 's/| Mo[a-z]* *\| [La-z]* *\| [Ba-z]* | [Ma-z]* |/''/g' -e 's/^| st[a-z]* *| *[[:digit:]]*.*//g' readme.md > readme.bak
#
# cat readme.bak | awk 'BEGIN{RS="\n\n" ; ORS=" ";}{ print }'
#
#
# #
# # LINENUM="$(grep -n "Generating report" readme.coverage.bak | head -n 1 | cut -d: -f1)"
# # TOREPLACE="$(cat readme.coverage.bak | awk '{ if ( NR > '$LINENUM'  ) { print } }')"
# #
# # echo $TOREPLACE
# #
# # TOREPLACE2="$(echo $TOREPLACE | awk '{ if ( NR > 1  ) { print } }')"
# #
# # echo $TOREPLACE2
# #
# # #
# # # hello=ho02123ware38384you443d34o3434ingtod38384day
# # # re='(.*)[0-9]+(.*)'
# # # while [[ $hello =~ $re ]]; do
# # #   hello=${BASH_REMATCH[1]}${BASH_REMATCH[2]}
# # # done
# # # echo "$hello"
