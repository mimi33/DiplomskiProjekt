import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp
import operator as op

greskePoGen = dict()
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	brojGen = int(ET.parse("Config.xml").find("Algorithm").find("Termination").find("Entry").text)
	vrijednosti = []
	for batchNode in ET.parse("./Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
		for jedinka in batchNode.find("XValidation").findall("Jedinka"):
			greska = float(jedinka.get("validationError"))
			generacija = int(jedinka.get("iteracija"))
			#print sat, batchNode.get("number"), generacija, greskePoGen
			if generacija not in greskePoGen:
				greskePoGen[generacija] = []
			greskePoGen[generacija].append(greska)


#plt.title()
plt.xticks(range(5, 201, 5), range(5, 201, 5))
plt.xlabel("Broj generacija")
plt.ylabel("Greska jedinke")
x= sorted(greskePoGen.keys())
y = [greskePoGen[k] for k in x]
box = plt.boxplot(y)
plt.figure()
plt.xticks(range(5, 201, 5), range(5, 201, 5))
plt.xlabel("Broj generacija")
plt.ylabel("Greska jedinke")
x= sorted(greskePoGen.keys())
y = [greskePoGen[k] for k in x]
plt.boxplot(y,0,'')

sortedkeys = x[:]
med = [b.get_ydata()[0] for b in box["medians"]]
q3 = [b.get_ydata()[0] for b in box["boxes"]]
q1 = [b.get_ydata()[2] for b in box["boxes"]]
caps = [b.get_ydata() for b in box["caps"]]
out1 = [b[0] for b in caps[::2]]
out3 = [b[0] for b in caps[1::2]]
outDiff = map(op.sub, out1, out3)
qDiff = map(op.sub, q1, q3)
avg = [sum(greskePoGen[k])/len(greskePoGen[k]) for k in x]
Min = [min(greskePoGen[k]) for k in x]
Max = [max(greskePoGen[k]) for k in x]

with open("stats.csv","w") as dat:
	dat.write("br.gen.;"+";".join(map(str, x)))
	dat.write("\nmax")
	dat.write(";"+";".join(map(str, Max)))
	dat.write("\ngg")
	dat.write(";"+";".join(map(str, out1)))
	dat.write("\nq1")
	dat.write(";"+";".join(map(str, q1)))
	dat.write("\nmed")
	dat.write(";"+";".join(map(str, med)))
	dat.write("\nq3")
	dat.write(";"+";".join(map(str, q3)))
	dat.write("\ndg")
	dat.write(";"+";".join(map(str, out3)))


plt.show()
