import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp
import operator as op
from mpl_toolkits.mplot3d import Axes3D
import numpy as np

def setBoxColors(bp):
	plt.setp(bp['boxes'][0], color='blue')
	plt.setp(bp['caps'][0], color='blue')
	plt.setp(bp['caps'][1], color='blue')
	plt.setp(bp['whiskers'][0], color='blue')
	plt.setp(bp['whiskers'][1], color='blue')
	plt.setp(bp['fliers'][0], color='blue')
	plt.setp(bp['fliers'][1], color='blue')
	plt.setp(bp['medians'][0], color='blue')

	plt.setp(bp['boxes'][1], color='red')
	plt.setp(bp['caps'][2], color='red')
	plt.setp(bp['caps'][3], color='red')
	plt.setp(bp['whiskers'][2], color='red')
	plt.setp(bp['whiskers'][3], color='red')
	plt.setp(bp['fliers'][2], color='red')
	plt.setp(bp['fliers'][3], color='red')
	plt.setp(bp['medians'][1], color='red')

maxGeneracija = 50
normiranje = True
legend = True

minDubinaUcenje = dict()
minDubinaProvjera = dict()
maxDubinaUcenje = dict()
maxDubinaProvjera = dict()
ucenje = dict()
provjera = dict()
for sat in range(24): #za svaki sat
	poSatuUcenje = dict()
	poSatuProvjera = dict()
	for j in range(1, 15): #za svake postavke
		# if (j == 7 or j == 10) and sat == 16:
		# 	continue
		minD = int(ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("MinDepth").text)
		maxD = int(ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("MaxDepth").text)
		key = minD, maxD

		if minD not in minDubinaProvjera:
			minDubinaProvjera[minD] = []
			minDubinaUcenje[minD] = []
		if maxD not in maxDubinaProvjera:
			maxDubinaProvjera[maxD] = []
			maxDubinaUcenje[maxD] = []
		if key not in provjera:
			provjera[key] = []
			ucenje[key] = []

		poSatuUcenje[(minD,maxD)] = []
		poSatuProvjera[(minD,maxD)] = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			for jedinka in batchNode.find("XValidation").findall("Jedinka"):
				if int(jedinka.get("iteracija")) == maxGeneracija:
					greskaucenje = float(jedinka.get("greska"))
					validacija = float(jedinka.get("validationError"))
					# if validacija > 1000:
					# 	print sat, batchNode.get("number"), validacija
					# 	continue
					poSatuUcenje[(minD, maxD)].append(greskaucenje)
					poSatuProvjera[(minD, maxD)].append(validacija)

	if normiranje:
		zbroj = sum([sum(k) for k in poSatuUcenje.values()]) + sum([sum(k) for k in poSatuProvjera.values()])
		broj  = sum([len(k) for k in poSatuUcenje.values()]) + sum([len(k) for k in poSatuProvjera.values()])
		prosjek = zbroj/broj
		print prosjek
		if prosjek > 5:
			print sat
		for k in poSatuUcenje:
			for l in range(len(poSatuUcenje[k])):
				poSatuUcenje[k][l] /= prosjek
			if k[0] not in minDubinaUcenje:
				minDubinaUcenje[k[0]] = []
			minDubinaUcenje[k[0]].extend(poSatuUcenje[k])
			if k[1] not in maxDubinaUcenje:
				maxDubinaUcenje[k[1]] = []
			maxDubinaUcenje[k[1]].extend(poSatuUcenje[k])
			for l in range(len(poSatuProvjera[k])):
				poSatuProvjera[k][l] /= prosjek
			if k[0] not in minDubinaProvjera:
				minDubinaProvjera[k[0]] = []
			minDubinaProvjera[k[0]].extend(poSatuProvjera[k])
			if k[1] not in maxDubinaProvjera:
				maxDubinaProvjera[k[1]] = []
			maxDubinaProvjera[k[1]].extend(poSatuProvjera[k])
			provjera[k].extend(poSatuProvjera[k])
			ucenje[k].extend(poSatuUcenje[k])
	else:
		for k in poSatuUcenje:
			if k[0] not in minDubinaUcenje:
				minDubinaUcenje[k[0]] = []
			minDubinaUcenje[k[0]].extend(poSatuUcenje[k])
			if k[1] not in maxDubinaUcenje:
				maxDubinaUcenje[k[1]] = []
			maxDubinaUcenje[k[1]].extend(poSatuUcenje[k])
			if k[0] not in minDubinaProvjera:
				minDubinaProvjera[k[0]] = []
			minDubinaProvjera[k[0]].extend(poSatuProvjera[k])
			if k[1] not in maxDubinaProvjera:
				maxDubinaProvjera[k[1]] = []
			maxDubinaProvjera[k[1]].extend(poSatuProvjera[k])
			provjera[k].extend(poSatuProvjera[k])
			ucenje[k].extend(poSatuUcenje[k])


plt.figure()
ax = plt.axes()
#plt.title("min dubina - skup za ucenje vs skup za provjeru")
plt.ylabel("Normirana greska jedinke")
plt.xlabel("Minimalna dubina stabla")
plt.grid(color="gray")
dubine = sorted(minDubinaUcenje.keys())
tickPos = [3*i+1.5 for i in range(len(dubine))]

for d in dubine:
	y = [minDubinaUcenje[d], minDubinaProvjera[d]]
	i = dubine.index(d)
	bp = plt.boxplot(y,0,"", positions = [3*i+1, 3*i+2], widths = 0.6)
	setBoxColors(bp)

plt.xlim(0,12)
plt.ylim(0.2,2)
ax.set_xticklabels([str(d) for d in dubine])
ax.set_xticks(tickPos)

if legend:
	hB, = plt.plot([1,1],'b-')
	hR, = plt.plot([1,1],'r-')
	plt.legend((hB, hR),('Ucenje', 'Provjera'))
	hB.set_visible(False)
	hR.set_visible(False)
# plt.show()

plt.figure()
ax = plt.axes()
#plt.title("max dubina - skup za ucenje vs skup za provjeru")
plt.ylabel("Normirana greska jedinke")
plt.xlabel("Maksimalna dubina stabla")
plt.grid(color="gray")

dubine = sorted(maxDubinaUcenje.keys())
tickPos = [3*i+1.5 for i in range(len(dubine))]

for d in dubine:
	y = [maxDubinaUcenje[d], maxDubinaProvjera[d]]
	i = dubine.index(d)
	bp = plt.boxplot(y,0,"", positions = [3*i+1, 3*i+2], widths = 0.6)
	setBoxColors(bp)

plt.xlim(0,12)
plt.ylim(0.2,2)
ax.set_xticklabels([str(d) for d in dubine])
ax.set_xticks(tickPos)

if legend:
	hB, = plt.plot([1,1],'b-')
	hR, = plt.plot([1,1],'r-')
	plt.legend((hB, hR),('Ucenje', 'Provjera'))
	hB.set_visible(False)
	hR.set_visible(False)
#plt.show()

plt.figure()
ax = plt.axes()
plt.title("skup za ucenje vs skup za provjeru")
plt.grid(color="gray")

dubine = sorted(ucenje.keys())
for d in dubine:
	y = [ucenje[d], provjera[d]]
	i = dubine.index(d)
	bp = plt.boxplot(y,0,"", positions = [3*i+1, 3*i+2], widths = 0.6)
	setBoxColors(bp)

plt.xlim(0,3*14)
#plt.ylim(0,2)
ax.set_xticklabels(dubine)
ax.set_xticks([3*i+1.5 for i in range(len(dubine))])

if legend:
	hB, = plt.plot([1,1],'b-')
	hR, = plt.plot([1,1],'r-')
	plt.legend((hB, hR),('Ucenje', 'Provjera'))
	hB.set_visible(False)
	hR.set_visible(False)
plt.show()


# plt.xticks(dubine, dubine)
# box = plt.boxplot(y1, 0, '')
# plt.subplot(212)
# plt.title("min dubina - skup za provjeru")
# dubine = sorted(minDubinaProvjera.keys())
# y1 = [minDubinaProvjera[k] for k in dubine]
# plt.xticks(dubine, dubine)
# box = plt.boxplot(y1, 0, '')
# plt.show()