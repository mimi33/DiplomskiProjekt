import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt

plotati = True

x = []
najmanji = []
najveci = []
prosjek = []
for sat in range(24): #za svaki sat
	vrijednosti = []
	for batchNode in ET.parse("./Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
		jedinka = batchNode.findall("Jedinka")[-1]
		vrijednosti.append(float(jedinka.get("greska")))
	x.append(sat)
	najmanji.append(min(vrijednosti))
	najveci.append(max(vrijednosti))
	prosjek.append(sum(vrijednosti)/len(vrijednosti))

plt.figure()
plt.xlabel('sat u danu')
plt.ylabel('Normirana Greska Jedinke')
plt.title("Greska u 24h")
p1, = plt.plot(x, najmanji)
p2, = plt.plot(x, najveci)
p3, = plt.plot(x, prosjek)
plt.xticks(range(24), range(24))
#plt.savefig("../Greska u 24h.png")
plt.show()