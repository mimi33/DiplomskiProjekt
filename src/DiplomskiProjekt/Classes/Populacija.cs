using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiplomskiProjekt.Classes
{
    public class Populacija : IList<Jedinka>
    {
        readonly List<Jedinka> _populacija;
        public int BrojJedinki;

        private Jedinka _najboljaJedinka;
        public Jedinka NajboljaJedinka
        {
            set
            {
                if (_najboljaJedinka == null || value.GreskaJedinke < _najboljaJedinka.GreskaJedinke)
                    _najboljaJedinka = value;
            }
            get
            {
                if (_najboljaJedinka == null)
                {
                    var min = _populacija.Min(x => x.GreskaJedinke);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    _najboljaJedinka = _populacija.First(x => x.GreskaJedinke == min);
                }
                return _najboljaJedinka;
            }
        }

        public Populacija()
        {
            _populacija = new List<Jedinka>();
        }

        /// <summary>
        /// Generira novu random populaciju
        /// </summary>
        public Populacija(int velicinaPopulacije)
        {
            BrojJedinki = velicinaPopulacije;
            _najboljaJedinka = null;

            _populacija = new List<Jedinka>();
            for (var i = 0; i < BrojJedinki; i++)
            {
                var j = new Jedinka();
                GP.EvaluationOp.Evaluiraj(j);
                NajboljaJedinka = j;
                _populacija.Add(j);
            }
        }

        public bool ProvijeriPreuzimanje() // todo ovo ne bi smijelo trebati
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var najgoraJedinka = _populacija.First(j => j.GreskaJedinke == _populacija.Min(x => x.GreskaJedinke));
            return
                (string.Compare(NajboljaJedinka.ToString(), najgoraJedinka.ToString(), StringComparison.InvariantCulture) == 0);
        }

        #region IList wrapper

        public int IndexOf(Jedinka item)
        {
            return _populacija.IndexOf(item);
        }

        public void Insert(int index, Jedinka item)
        {
            _populacija.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _populacija.RemoveAt(index);
        }

        public Jedinka this[int index]
        {
            get { return _populacija[index]; }
            set { _populacija[index] = value; }
        }

        public void Add(Jedinka jedinka)
        {
            _populacija.Add(jedinka);
        }

        public void Clear()
        {
            _populacija.Clear();
        }

        public bool Contains(Jedinka jedinka)
        {
            return _populacija.Contains(jedinka);
        }

        public void CopyTo(Jedinka[] array, int arrayIndex)
        {
            _populacija.CopyTo(array, arrayIndex);
        }

        public bool Remove(Jedinka item)
        {
            return _populacija.Remove(item);
        }

        public int Count
        {
            get { return _populacija.Count; } 
        }

        public bool IsReadOnly
        {
            get {return false;}
        }

        public IEnumerator<Jedinka> GetEnumerator()
        {
            return _populacija.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
    