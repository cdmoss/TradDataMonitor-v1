!#bin/bash
if test -f /home/pi/Trad-Data-Monitor/TradPackage/graph.png; then
	rm -rf /home/pi/Trad-Data-Monitor/TradPackage/chartbuilder.py
fi

python3 /home/pi/Trad-Data-Monitor/TradPackage/chartbuilder.py