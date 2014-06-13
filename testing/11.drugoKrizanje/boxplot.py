import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import re
mjerenjaPoSatima = dict()
mjerenja = {}
imenaCRXS = dict()
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	for j in range(1, 3): #za svake postavke
		crxName = ET.parse("./"+str(j)+"/Config.xml").find("Crossover").find("Name").text
		imenaCRXS[j] = crxName
		imenaCRXS[j] = "\n".join(re.findall('[A-Z][^A-Z]*', crxName))
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

plt.subplot(121)
plt.title("Sa odstupanjima")
plt.ylabel("Normirana greska jedinke")
plt.grid(color='gray')
x = sorted(imenaCRXS.keys())
u = [mjerenja[k] for k in x]
plt.ylim(0.5,2.5)
plt.xticks(imenaCRXS.keys(),imenaCRXS.values())
bp = plt.boxplot(u)

plt.subplot(122)
plt.title("Bez odstupanja")
x = sorted(imenaCRXS.keys())
u = [mjerenja[k] for k in x]
plt.grid(color='gray')
plt.xticks(imenaCRXS.keys(),imenaCRXS.values())
bp = plt.boxplot(u,0,'')

plt.show()
