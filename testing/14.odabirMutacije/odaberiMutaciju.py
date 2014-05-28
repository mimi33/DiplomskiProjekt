import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

mjerenjaPoSatima1 = dict()
mjerenjaPoSatima2 = dict()
for sat in range(24): #za svaki sat
	svaMjerenja1 = dict()
	for j in range(1, 6): #za svake postavke
		k = ET.parse("./"+str(j)+"/Config.xml").find("Mutation").find("MutFactor").text
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja1[k] = vrijednosti

	svaMjerenja2 = dict()
	for j in range(6, 11): #za svake postavke
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

	mjerenjaPoSatima1[sat] = svaMjerenja1
	mjerenjaPoSatima2[sat] = svaMjerenja2
#mjerenjaPoSatima[sat][crxFact][batchNo]

mjerenjaPoParametru1 = dict()
for sat in mjerenjaPoSatima1:
	for vp in mjerenjaPoSatima1[sat]:
		if vp not in mjerenjaPoParametru1:
			mjerenjaPoParametru1[vp] = []
		mjerenjaPoParametru1[vp].extend(mjerenjaPoSatima1[sat][vp])
mjerenjaPoParametru2 = dict()
for sat in mjerenjaPoSatima2:
	for vp in mjerenjaPoSatima2[sat]:
		if vp not in mjerenjaPoParametru2:
			mjerenjaPoParametru2[vp] = []
		mjerenjaPoParametru2[vp].extend(mjerenjaPoSatima2[sat][vp])
#mjerenjaPoVelPop[crxFact]

najmanji = []
najveci = []
prosjek = []
index = []
for vp in mjerenjaPoParametru1:
	vrijednosti = mjerenjaPoParametru1[vp]
	index.append(vp)
	najmanji.append(min(vrijednosti))
	najveci.append(max(vrijednosti))
	prosjek.append(sum(vrijednosti)/len(vrijednosti))

najmanji2 = []
najveci2 = []
prosjek2 = []
index2 = []
for vp in mjerenjaPoParametru1:
	vrijednosti = mjerenjaPoParametru2[vp]
	index2.append(vp)
	najmanji2.append(min(vrijednosti))
	najveci2.append(max(vrijednosti))
	prosjek2.append(sum(vrijednosti)/len(vrijednosti))
pp.pprint(mjerenjaPoParametru1)
print index
print najmanji
print prosjek
print najveci

plt.figure()
plt.xlabel('faktor mutacije')
plt.ylabel('Normirana Greska Jedinke')
plt.title("Mutacija")
x,y = zip(*sorted(zip(index, najmanji)))
p1, = plt.plot(x, y, "r--")
x,y = zip(*sorted(zip(index, najveci)))
p2, = plt.plot(x, y, "r:")
x,y = zip(*sorted(zip(index, prosjek)))
p3, = plt.plot(x, y, "r")

x,y = zip(*sorted(zip(index2, najmanji2)))
p4, = plt.plot(x, y, "b--")
x,y = zip(*sorted(zip(index2, najveci2)))
p5, = plt.plot(x, y, "b:")
x,y = zip(*sorted(zip(index2, prosjek2)))
p6, = plt.plot(x, y, "b")
plt.legend([p3,p6], ["point", "hoist"])
#plt.savefig("UkupnaSlikaZaOdabirMutacije.png")
plt.show()
