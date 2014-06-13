import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

mjerenja1 = dict()
mjerenja2 = dict()
for sat in range(24): #za svaki sat
	svaMjerenja1 = dict()
	for j in range(1, 7): #za svake postavke
		k = ET.parse("./"+str(j)+"/Config.xml").find("Mutation").find("MutFactor").text
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja1[k] = vrijednosti

	svaMjerenja2 = dict()
	for j in range(7, 13): #za svake postavke
		k = ET.parse("./"+str(j)+"/Config.xml").find("Mutation").find("MutFactor").text
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja2[k] = vrijednosti

	ukupnaGreska = sum([sum(m) for m in svaMjerenja1.values()]) + sum([sum(m) for m in svaMjerenja2.values()])
	brojGreski = sum([len(m) for m in svaMjerenja1.values()]) +sum([len(m) for m in svaMjerenja2.values()])
	srednjaGreska = ukupnaGreska/brojGreski
	for m in svaMjerenja1.values():
		for i in range(len(m)):
			m[i] /= srednjaGreska
	for m in svaMjerenja2.values():
		for i in range(len(m)):
			m[i] /= srednjaGreska

	for j in svaMjerenja1:
		if j not in mjerenja1: mjerenja1[j] = []
		mjerenja1[j].extend(svaMjerenja1[j])
	for j in svaMjerenja2:
		if j not in mjerenja2: mjerenja2[j] = []
		mjerenja2[j].extend(svaMjerenja2[j])


plt.figure()
ax = plt.axes()
plt.title("Point mutation")
plt.ylabel("Normirana greska jedinke")
plt.xlabel("Vjerojatnost mutacije")
plt.grid(color='gray')
x = sorted(mjerenja1.keys())
u = [mjerenja1[k] for k in x]
plt.xticks(x,x)
bp = plt.boxplot(u)

plt.figure()
ax = plt.axes()
plt.title("Hoist mutation")
plt.ylabel("Normirana greska jedinke")
plt.xlabel("Vjerojatnost mutacije")
x = sorted(mjerenja2.keys())
u = [mjerenja2[k] for k in x]
plt.grid(color='gray')
plt.xticks(x,x)
bp = plt.boxplot(u)

plt.show()

