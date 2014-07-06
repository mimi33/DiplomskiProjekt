import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
from random import random

plotati = True

color = [10*random() for i in range(240)]

x = []
y = []
for sat in range(24): #za svaki sat
	vrijednosti = []
	for batchNode in ET.parse("./1/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
		for XV in batchNode.findall("XValidation"):
			jedinka = XV.findall("Jedinka")[-1]
			y.append(float(jedinka.get("greska")))
			x.append(sat)
plt.figure()
plt.xlabel('sat u danu')
plt.ylabel('Greska Jedinke')
plt.title("Greska u 24h")
plt.grid(color="gray")
plt.xlim(0,23)
plt.ylim(0,10)
plt.scatter(x,y, c=y, marker='+', cmap=plt.cm.jet)
plt.xticks(range(24), range(24))
#plt.savefig("../Greska u 24h.png")
plt.show()