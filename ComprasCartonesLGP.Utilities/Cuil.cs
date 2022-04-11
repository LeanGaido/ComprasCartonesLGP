using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Utilities
{
    public class Cuil
    {
        private string _Cuil = string.Empty;
        private bool _Valido = false;

        public Cuil()
        {
        }

        public Cuil(string CadenaCuit)
        {
            _Cuil = CadenaCuit;
            _Valido = CUITValido();
        }

        public string CUITSinFormato
        {
            get
            {
                return _Cuil;
            }
            set
            {
                _Cuil = value;
                _Valido = CUITValido();
            }
        }

        public string CUITFormateado
        {
            get
            {
                if (!_Valido) return string.Empty;
                if (_Cuil.Length == 0) return string.Empty;
                return _Cuil.Substring(0, 2) + "-" +
                       _Cuil.Substring(2, 8) + "-" +
                       _Cuil.Substring(10);
            }
        }

        public bool EsValido
        {
            get
            {
                return _Valido;
            }
        }

        private bool CUITValido()
        {
            if (_Cuil.Length == 0) return true;
            string CUITValidado = string.Empty;
            bool Valido = false;
            char Ch;
            for (int i = 0; i < _Cuil.Length; i++)
            {
                Ch = _Cuil[i];
                if ((Ch > 47) && (Ch < 58))
                {
                    CUITValidado = CUITValidado + Ch;
                }
            }

            _Cuil = CUITValidado;
            Valido = (_Cuil.Length == 11);
            if (Valido)
            {
                int Verificador = EncontrarVerificador(_Cuil);
                Valido = (_Cuil[10].ToString() == Verificador.ToString());
            }

            return Valido;
        }

        private int EncontrarVerificador(string CUIT)
        {
            int Sumador = 0;
            int Producto = 0;
            int Coeficiente = 0;
            int Resta = 5;
            for (int i = 0; i < 10; i++)
            {
                if (i == 4) Resta = 11;
                Producto = CUIT[i];
                Producto -= 48;
                Coeficiente = Resta - i;
                Producto = Producto * Coeficiente;
                Sumador = Sumador + Producto;
            }

            int Resultado = Sumador - (11 * (Sumador / 11));
            Resultado = 11 - Resultado;

            if (Resultado == 11) return 0;
            else return Resultado;
        }
    }
}
