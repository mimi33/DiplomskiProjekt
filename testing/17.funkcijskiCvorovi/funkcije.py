import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp
import operator as op
from mpl_toolkits.mplot3d import Axes3D
import numpy as np
from scipy.stats import gaussian_kde

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

def violin_plot(ax,data,pos, bp=False):
	'''
	create violin plots on an axis
	'''
	dist = max(pos)-min(pos)
	w = min(0.15*max(dist,1.0),0.5)
	for d,p in zip(data,pos):
		k = gaussian_kde(d) #calculates the kernel density
		m = k.dataset.min() #lower bound of violin
		M = k.dataset.max() #upper bound of violin
		x = np.arange(m,M,(M-m)/100.) # support for violin
		v = k.evaluate(x) #violin profile (density curve)
		v = v/v.max()*w #scaling the violin to the available space
		ax.fill_betweenx(x,p,v+p,facecolor='y',alpha=0.3)
		ax.fill_betweenx(x,p,-v+p,facecolor='y',alpha=0.3)
	if bp:
		ax.boxplot(data,positions=pos,vert=1)

maxGeneracija = 50
normiranje = True
legend = False

greskeUcenje = dict()
greskeProvjera = dict()
for sat in range(24): #za svaki sat
	greskePoSatuUcenje = dict()
	greskePoSatuProvjera = dict()
	for j in range(7): #za svake postavke
		fun = ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("FunctionSet").text.split(' ')[-1]
		if fun == "ifRadniDan":
			fun = "---"
		greskePoSatuUcenje[fun] = []
		greskePoSatuProvjera[fun] = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			for jedinka in batchNode.find("XValidation").findall("Jedinka"):
				if int(jedinka.get("iteracija")) == maxGeneracija:
					ucenjegreska = float(jedinka.get("greska"))
					validacijaGreska= float(jedinka.get("validationError"))
					greskePoSatuUcenje[fun].append(ucenjegreska)
					greskePoSatuProvjera[fun].append(validacijaGreska)

	if normiranje:
		zbroj = sum([sum(k) for k in greskePoSatuUcenje.values()])# + sum([sum(k) for k in greskePoSatuProvjera.values()])
		broj = sum([len(k) for k in greskePoSatuUcenje.values()])#+ sum([len(k) for k in greskePoSatuProvjera.values()])
		prosjek = zbroj/broj
		print prosjek
		if prosjek > 5:
			print sat
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



plt.figure()
plt.title("greske")
plt.grid(color="gray")
funkcije = sorted(greskeUcenje.keys())
y1 = [greskeUcenje[k] for k in funkcije]
plt.xticks(range(12), funkcije)
box = plt.boxplot(y1)#, 0, '')

plt.figure()
plt.title("violin plot")
plt.grid(color="gray")
ax = plt.axes()
violin_plot(ax, y1, range(len(funkcije)), True)
ax.set_xticklabels(funkcije)


sortedkeys = funkcije[:]
med = [b.get_ydata()[0] for b in box["medians"]]
q3 = [b.get_ydata()[0] for b in box["boxes"]]
q1 = [b.get_ydata()[2] for b in box["boxes"]]
caps = [b.get_ydata() for b in box["caps"]]
out1 = [b[0] for b in caps[::2]]
out3 = [b[0] for b in caps[1::2]]
outDiff = map(op.sub, out1, out3)
qDiff = map(op.sub, q1, q3)
avg = [sum(greskeUcenje[k])/len(greskeUcenje[k]) for k in sortedkeys]


plt.figure()
ax = plt.axes()
plt.grid(color="gray")
plt.ylabel("Normirana greska jedinke")
plt.title("Greske sortirane po medijanu")
x = [k for (m,k) in sorted(zip(med,sortedkeys))]
u = [greskeUcenje[k] for k in x]
bp = plt.boxplot(u, 0, '')

#plt.ylim(0,2)
ax.set_xticklabels(x)
# ax.set_xticks([3*i+1.5 for i in range(len(x))])

plt.figure()
ax = plt.axes()
plt.grid(color="gray")

x = [k for (m,k) in sorted(zip(med,sortedkeys))]
y1 =[greskeUcenje[k] for k in x]
y2 = [greskeProvjera[k] for k in x]

for i in range(len(x)):
	y = [y1[i], y2[i]]
	bp = plt.boxplot(y, positions = [3*i+1, 3*i+2], widths = 0.6)
	setBoxColors(bp)

plt.xlim(0,3*len(x))
#if normiranje: plt.ylim(0,2.5)
ax.set_xticklabels(x)
ax.set_xticks([3*i+1.5 for i in range(len(x))])

if legend:
	hB, = plt.plot([1,1],'b-')
	hR, = plt.plot([1,1],'r-')
	plt.legend((hB, hR),('Ucenje', 'Provjera'))
	hB.set_visible(False)
	hR.set_visible(False)


plt.show()
