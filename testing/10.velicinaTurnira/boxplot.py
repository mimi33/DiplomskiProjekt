import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

mjerenjaPoSatima = dict()
mjerenja = {}
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	for j in range(1, 6): #za svake postavke
		k = int(ET.parse("./"+str(j)+"/Config.xml").find("Algorithm").find("ParamK").text)
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja[k] = vrijednosti

	ukupnaGreska = sum([sum(m) for m in svaMjerenja.values()])
	brojGreski = sum([len(m) for m in svaMjerenja.values()])
	srednjaGreska = ukupnaGreska/brojGreski
	for m in svaMjerenja.values():
		for i in range(len(m)):
			m[i] /= srednjaGreska

	for j in svaMjerenja:
		if j not in mjerenja: mjerenja[j] = []
		mjerenja[j].extend(svaMjerenja[j])



plt.figure()
ax = plt.axes()
plt.title("Velicina turnira")
plt.ylabel("Normirana greska jedinke")
plt.grid(color='gray')
x = sorted(mjerenja.keys())
u = [mjerenja[k] for k in x]
plt.xticks(x,x)
bp = plt.boxplot(u)

plt.figure()
ax = plt.axes()
plt.xlabel("Velicina turnira")
plt.ylabel("Normirana greska jedinke")
x = sorted(mjerenja.keys())
u = [mjerenja[k] for k in x]
plt.grid(color='gray')
plt.xticks(x,x)
bp = plt.boxplot(u,0,'')

plt.show()
