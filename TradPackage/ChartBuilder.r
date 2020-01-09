library(tidyverse)
library(lubridate)

df = read_csv("C:/Users/chase.mossing2/Desktop/TradPackage/data.csv")

p = df %>% 
  ggplot(aes(x=as.POSIXct(ymd_hms(DateTime)), 
             y=Data, 
             group=SensorType, 
             color=SensorType)) +
  geom_line() +
  scale_x_datetime() +
  labs(x = "Time of Reading") + 
  labs(y = "Sensor Data") + 
  labs(color = "Sensor Type")

png(filename = "C:/Users/chase.mossing2/Desktop/TradPackage/graph.png", width = 640, height = 480)
p
dev.off()