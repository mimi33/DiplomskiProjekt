import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

imenaAlgoritama = {1: "Eliminacijski\n2 operatora",
				   2: "Eliminacijski\n1 operator",
				   3: "Generacijski\n1 operator",
				   4: "Generacijski\n2 operatora"}
mjerenja = dict()
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	for j in range(1, 5): #za svake postavke
		ime = ET.parse("./"+str(j)+"/Config.xml").find("Algorithm").get("name")
		#if j not in imenaAlgoritama: imenaAlgoritama[j] = ime
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja[j] = vrijednosti

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
plt.title("Algoritmi")
plt.ylabel("Normirana greska jedinke")
x = imenaAlgoritama.keys()
u = [mjerenja[k] for k in x]
plt.xticks(imenaAlgoritama.keys(), imenaAlgoritama.values())
bp = plt.boxplot(u)

plt.figure()
ax = plt.axes()
plt.title("Algoritmi")
plt.ylabel("Normirana greska jedinke")
x = imenaAlgoritama.keys()
u = [mjerenja[k] for k in x]
plt.xticks(imenaAlgoritama.keys(), imenaAlgoritama.values())
bp = plt.boxplot(u,0,'')

plt.show()
