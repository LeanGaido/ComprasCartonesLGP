using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComprasCartonesLGP.Entities
{
    public class Asociado
    {
        public int ID { get; set; }

        public string NumeroDeAsociado { get; set; }//Número de Asociado: Se consulta por Sexo y Nro. De DNI. Si existe se carga el número de asociado, en caso que no exista informar 5 ceros. *

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string NombreCompleto
        { 
            get
            {
                return Apellido + ", " + Nombre;
            }
        }

        public DateTime FechaNacimiento { get; set; }//3) Fecha de Nacimiento: AAAA-MM-DD.- *

        public string Sexo { get; set; }//17) Sexo: 1 F o 2 M . *

        public string Cuit { get; set; }//20) CUIT: 11 caracteres numéricos: CUIT Validado

        public string Dni { get; set; }//18) DNI: 10 caracteres numéricos: *

        public string Direccion { get; set; }//5) Dirección: 25 caracteres.-

        public int Altura { get; set; }//6) Número: 5 caracteres.-

        public string Torre { get; set; }

        public string Piso { get; set; }//8) Piso: No necesario.-

        public string Dpto { get; set; }//9) Departamento: No necesario.-

        public string Barrio { get; set; }//10) Barrio: No necesario.-

        [ForeignKey("Localidad")]
        public int LocalidadID { get; set; }//11) Código Postal: POR CODIGO. *

        public string AreaTelefonoFijo { get; set; }//26) Prefijo Celular: Ej: 3564

        public string NumeroTelefonoFijo { get; set; }//27) Número Celular: Ej: 585858

        public string Email { get; set; }//14) Email: 40 caracteres.-

        public string AreaCelular { get; set; }//24) Prefijo Celular: Ej: 3564 *

        public string NumeroCelular { get; set; }//25) Número Celular: Ej: 585858 *

        public string AreaCelularAux { get; set; }//26) Prefijo Celular: Ej: 3564

        public string NumeroCelularAux { get; set; }//27) Número Celular: Ej: 585858

        public int TipoDeAsociado { get; set; }//16) Tipo de Asociado: Fijo 1

        public DateTime FechaAlta { get; set; }//23) Fecha de Alta: AAAA-MM-DD *

        public virtual Localidades Localidad { get; set; }
    }
}