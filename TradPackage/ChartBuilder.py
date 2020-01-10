import pandas as pd
from matplotlib import pyplot as plt

df =  pd.read_csv('/home/pi/Trad-Data-Monitor/TradPackage/data.csv')

p = df.plot('DateTime', 'Data', kind = 'line')
p.set_xlabel("Time")
p.set_ylabel("Sensor Data")


plt.savefig('/home/pi/Trad-Data-Monitor/TradPackage/graph.png')