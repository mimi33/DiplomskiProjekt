import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp
import operator as op

maxGeneracija = 50
normiranje = True
legend = False

greskeUcenje = dict()
for sat in range(24): #za svaki sat
	greskePoSatuUcenje = dict()
	mjerenja = range(8)
	mjerenja.extend(range(10,15))
	mjerenja.extend(range(20,24))
	for j in mjerenja: #za svake postavke
		fun = ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("FunctionSet").text.split(' ')
		fun.remove("+")
		fun.remove("-")
		fun.remove("*")
		fun.remove("/")
		fun.remove("ifRadniDan")
		if "id" in fun: fun.remove("id")
		if len(fun) == 0:
			fun.append("---")
		fun = tuple(fun)
		greskePoSatuUcenje[fun] = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			for jedinka in batchNode.find("XValidation").findall("Jedinka"):
				if int(jedinka.get("iteracija")) == maxGeneracija:
					ucenjegreska = float(jedinka.get("greska"))
					greskePoSatuUcenje[fun].append(ucenjegreska)

	if normiranje:
		zbroj = sum([sum(k) for k in greskePoSatuUcenje.values()])
		broj = sum([len(k) for k in greskePoSatuUcenje.values()])
		prosjek = zbroj/broj
		print prosjek
		if prosjek > 5:
			print sat
		for k in greskePoSatuUcenje:
			for l in range(len(greskePoSatuUcenje[k])):
				greskePoSatuUcenje[k][l] /= prosjek

	for k in greskePoSatuUcenje:
		if k not in greskeUcenje: greskeUcenje[k] = []
		greskeUcenje[k].extend(greskePoSatuUcenje[k])



plt.figure()
plt.title("greske")
plt.grid(color="gray")
funkcije = sorted(greskeUcenje.keys())
y1 = [greskeUcenje[k] for k in funkcije]
plt.xticks(range(12),["\n".join(f) for f in funkcije])
# ax.set_xticklabels([pp.pformat(f[5:], width=1) for f in x])
box = plt.boxplot(y1)#, 0, '')

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
print "avg\n", avg


plt.figure()
ax = plt.axes()
plt.grid(color="gray")
plt.title("Greske sortirane po medijanu")
plt.ylabel("Normirana greska jedinke")
x = [k for (m,k) in sorted(zip(med,sortedkeys))]
u = [greskeUcenje[k] for k in x]
bp = plt.boxplot(u, 0, '')

#plt.ylim(0,2)
ax.set_xticklabels(["\n".join(f) for f in x])

plt.show()
