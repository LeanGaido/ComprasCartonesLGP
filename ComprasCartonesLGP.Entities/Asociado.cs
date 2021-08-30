using System;

namespace ComprasCartonesLGP.Entities
{
    public class Asociado
    {
        public int ID { get; set; }

        public int NumeroDeAsociado { get; set; }//Número de Asociado: Se consulta por Sexo y Nro. De DNI. Si existe se carga el número de asociado, en caso que no exista informar 5 ceros. *

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Descripcion //2) Descripción: 30 caracteres.- *
        { 
            get
            {
                return Apellido + ", " + Nombre;
            }
        }

        public DateTime FechaNacimiento { get; set; }//3) Fecha de Nacimiento: AAAA-MM-DD.- *

        //4) Actividad: POR CODIGO. No necesario.-

        public string Direccion { get; set; }//5) Dirección: 25 caracteres.-

        public int Altura { get; set; }//6) Número: 5 caracteres.-

        //7) Torre: No necesario.-

        public string Piso { get; set; }//8) Piso: No necesario.-

        public string Dpto { get; set; }//9) Departamento: No necesario.-

        public string Barrio { get; set; }//10) Barrio: No necesario.-

        public int CodPostal { get; set; }//11) Código Postal: POR CODIGO. *

        public int Provincia { get; set; }//12) Provincia: POR CODIGO. *

        public string TelefonoFijo { get; set; }//13) Teléfono Fijo: 30 caracteres. *

        public string Email { get; set; }//14) Email: 40 caracteres.-

        public string Localidad { get; set; }//15) Localidad: 25 caracteres (viene de la descripción del punto 11). *

        public int TipoDeAsociado { get; set; }//16) Tipo de Asociado: Fijo 1

        public int Sexi { get; set; }//17) Sexo: 1 F o 2 M . *

        public string Numero { get; set; }//18) DNI: 10 caracteres numéricos: *

        public int Afip { get; set; }//19) Afip: Fijo 1

        public string cuit { get; set; }//20) CUIT: 11 caracteres numéricos: CUIT Validado

        public string IngresosBrutos { get; set; }//21) Ingresos Brutos: No necesario.

        public float Saldo { get; set; }//22) Saldo: Fijo 0

        public DateTime FechaAlta { get; set; }//23) Fecha de Alta: AAAA-MM-DD *

        public string AreaCelular { get; set; }//24) Prefijo Celular: Ej: 3564 *

        public string NumeroCelular { get; set; }//25) Número Celular: Ej: 585858 *

        public string AreaCelularAux { get; set; }//26) Prefijo Celular: Ej: 3564

        public string NumeroCelularAux { get; set; }//27) Número Celular: Ej: 585858

        public string NumeroSolicitud { get; set; }//28) NUMERO DE LA SOLICITUD COMPRADA. Ej: 00001 *

        public DateTime FechaDeCompra { get; set; }//29) FECHA DE COMPRA AAAA-MM-DD *

        public int FormaDePago { get; set; }//30) FORMA DE PAGO: Fijo 1 *

        public string Vendedor { get; set; }//31) Vendedor: Por Código 3 caracteres Ej: 003 *

        public string ComisionVendedor { get; set; }//32) Comisión Vendedor: Por Código 2 caracteres Ej: 03 *
    }
}