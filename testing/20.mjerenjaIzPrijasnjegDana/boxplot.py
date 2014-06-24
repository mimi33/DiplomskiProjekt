import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt

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

podaci = {1: "bez\nprijasnjih\nmjerenja",
		  2: "prijasnji\nsat",
		  3: "dva\nprijasnja\nsata",
		  4: "tri\nprijasnja\nsata",
		  5: "prosjek\nprijasnjih\n24sata"}

greskeUcenje = dict()
greskeProvjera = dict()
for sat in range(24): #za svaki sat
	greskePoSatuUcenje = dict()
	greskePoSatuProvjera = dict()
	for j in range(1,6): #za svake postavke
		greskePoSatuUcenje[j] = []
		greskePoSatuProvjera[j] = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			for XV in batchNode.findall("XValidation"):
				for jedinka in XV.findall("Jedinka"):
					if int(jedinka.get("iteracija")) == maxGeneracija:
						ucenjegreska = float(jedinka.get("greska"))
						validacijaGreska= float(jedinka.get("validationError"))
						greskePoSatuUcenje[j].append(ucenjegreska)
						greskePoSatuProvjera[j].append(validacijaGreska)

	if normiranje:
		zbroj = sum([sum(k) for k in greskePoSatuUcenje.values()]) + sum([sum(k) for k in greskePoSatuProvjera.values()])
		broj = sum([len(k) for k in greskePoSatuUcenje.values()]) + sum([len(k) for k in greskePoSatuProvjera.values()])
		prosjek = zbroj/broj
		for k in greskePoSatuUcenje:
			for l in range(len(greskePoSatuUcenje[k])):
				greskePoSatuUcenje[k][l] /= prosjek
			for l in range(len(greskePoSatuProvjera[k])):
				greskePoSatuProvjera[k][l] /= prosjek

	for k in greskePoSatuUcenje:
		if k not in greskeProvjera: greskeProvjera[k] = []
		greskeProvjera[k].extend(greskePoSatuProvjera[k])
		if k not in greskeUcenje: greskeUcenje[k] = []
		greskeUcenje[k].extend(greskePoSatuUcenje[k])

#
# plt.figure()
# plt.title("greske")
# plt.grid(color="gray")
# x = sorted(greskeUcenje.keys())
#
# y1 = [greskeUcenje[k] for k in x]
# plt.xticks(range(12), x)
# box = plt.boxplot(y1)#, 0, '')
#

plt.figure()
ax = plt.axes()
plt.grid(color="gray")
x = sorted(greskeUcenje.keys())
y1 =[greskeUcenje[k] for k in x]
y2 = [greskeProvjera[k] for k in x]

for i in range(len(x)):
	y = [y1[i], y2[i]]
	bp = plt.boxplot(y,0,"", positions = [3*i+1, 3*i+2], widths = 0.6)
	setBoxColors(bp)

plt.xlim(0,3*len(x))
plt.ylim(0,3.3)
plt.ylabel("Normirana greska jedinke")

ax.set_xticklabels([podaci[k] for k in sorted(podaci.keys())])
ax.set_xticks([3*i+1.5 for i in range(len(x))])

if legend:
	hB, = plt.plot([1,1],'b-')
	hR, = plt.plot([1,1],'r-')
	plt.legend((hB, hR),('Ucenje', 'Provjera'))
	hB.set_visible(False)
	hR.set_visible(False)


plt.show()
