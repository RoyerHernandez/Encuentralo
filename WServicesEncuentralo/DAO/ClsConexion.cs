using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace WServicesEncuentralo.DAO
{
    class ClsConexion : StoredProcedure
    {
        public DataTable EjecutarProcedimiento(string NombreConexion)
        {
            string NombreConexionString = ConfigurationManager.ConnectionStrings[NombreConexion].ConnectionString;
            SqlConnection SqlConect = new SqlConnection(NombreConexionString);
            SqlCommand SqlComando = new SqlCommand(Nombre, SqlConect);
            SqlComando.CommandTimeout = 200;
            SqlDataReader Reader = default(SqlDataReader);
            DataTable Tabla = new DataTable();
            SqlComando.CommandType = CommandType.StoredProcedure;

            StoredProcedureParameter TodosParametros = null;
            foreach (StoredProcedureParameter TodosParametros_loopVariable in this.Parametros)
            {
                TodosParametros = TodosParametros_loopVariable;
                SqlParameter NuevoParametro = new SqlParameter(TodosParametros.Parametro, TodosParametros.Valor)
                {
                    Direction = ParameterDirection.Input
                };
                SqlComando.Parameters.Add(NuevoParametro);
            }
            try
            {
                SqlConect.Open();
                Reader = SqlComando.ExecuteReader(CommandBehavior.CloseConnection);
                Tabla.Load(Reader);
            }
            catch (Exception ex)
            {
                string error = ex.ToString().ToUpper();

                if (error.Contains("ERROR NUMBER:18456") || error.Contains("ERROR NUMBER:4060") || error.Contains("ERROR NUMBER:-1"))
                {
                    throw new Exception("DBConnection");
                }

            }
            finally
            {
                SqlConect.Close();
            }
            return Tabla;
        }
    }

    public class StoredProcedure
    {

        #region "Variables"
        private string nombreProcedimiento;
        private List<StoredProcedureParameter> nombreParametros = new List<StoredProcedureParameter> { };
        private List<StoredProcedureParameter> nombreParametrosSalida = new List<StoredProcedureParameter> { };
        #endregion

        #region "Propiedades"

        public string Nombre
        {
            get { return nombreProcedimiento; }
            set { nombreProcedimiento = value; }
        }

        public List<StoredProcedureParameter> ParametroSalida
        {
            get { return nombreParametrosSalida; }
            set { nombreParametrosSalida = value; }
        }

        public List<StoredProcedureParameter> Parametros
        {
            get { return nombreParametros; }
            set { nombreParametros = value; }
        }
        #endregion

        #region "Propiedades y Funciones"

        /// <summary>
        /// Procedimiento que añade informacion de los parametros en un arreglo.
        /// </summary>
        /// <param name="nomParametro">Nombre del parametro</param>
        /// <param name="valParametro">Valor del parametro</param>
        /// <remarks></remarks> 
        public void AgregarParametro(string nomParametro, object valParametro)
        {
            StoredProcedureParameter parametro = new StoredProcedureParameter("@" + nomParametro, valParametro);
            this.Parametros.Add(parametro);
        }

        /// <summary>
        /// Procedimiento que añade informacion de los parametros de salida en un arreglo.
        /// </summary>
        /// <param name="nomParametro">Nombre del parametro</param>
        /// <param name="tipoParametro">Tipo de parametro</param>
        /// <param name="tamano">Tamano del parametro en el SP</param>
        /// <remarks></remarks> 
        public void AgregarParametroSalida(string nomParametro, object tipoParametro, int tamano)
        {
            StoredProcedureParameter parametro = new StoredProcedureParameter("@" + nomParametro, tipoParametro, tamano);
            this.ParametroSalida.Add(parametro);
        }

        #endregion

        #region "Constructor"
        public StoredProcedure()
        {
        }
        #endregion

    }

    public class StoredProcedureParameter
    {
        #region "Variables"
        private string nombreParametro;
        private object valorParametro;
        private int tipoParametro;
        #endregion

        #region "Propiedades"
        //Nombre de la variable, debe ser igual a la declarada en el procedimiento almacenado
        public string Parametro
        {
            get { return nombreParametro; }
            set { nombreParametro = value; }
        }

        //Valor de la variable, puede ser de cualquier tipo de dato. preferible que 
        //coincida con las variables declaradas en GetTypeProperty
        public object Valor
        {
            get { return valorParametro; }
            set { valorParametro = value; }
        }

        public int Tamano
        {
            get { return tipoParametro; }
            set { tipoParametro = value; }
        }
        //Se definen tipos datos del procedimiento
        //Agregar mas tipos dedatos si es necesario para el procedimiento.
        public SqlDbType GetTypeProperty
        {
            get
            {
                switch (valorParametro.GetType().FullName)
                {
                    case "System.Image":
                        return SqlDbType.Image;
                    case "System.String":
                        return SqlDbType.VarChar;
                    case "System.Int16":
                        return SqlDbType.SmallInt;
                    case "System.Int32":
                        return SqlDbType.Int;
                    case "System.Int64":
                        return SqlDbType.BigInt;
                    case "System.Decimal":
                        return SqlDbType.Decimal;
                    case "System.Double":
                        return SqlDbType.Float;
                    case "System.Boolean":
                        return SqlDbType.Bit;
                    case "System.DateTime":
                        return SqlDbType.DateTime;
                    case "System.Byte":
                        return SqlDbType.VarBinary;
                    case "System.Byte[]":
                        return SqlDbType.Image;
                    case "System.Data.SqlDbType":
                        return SqlDbType.VarBinary;
                    case "System.Data.SqlTypes.SqlBinary":
                        return SqlDbType.VarBinary;
                    default:
                        return SqlDbType.VarChar;
                }
            }
        }
        #endregion

        #region "Constructor"
        public StoredProcedureParameter(string nomParametro, object valParametro)
        {
            this.Parametro = nomParametro;
            this.Valor = valParametro;
        }

        public StoredProcedureParameter(string nomParametro, object valParametro, int tamanoParametro)
        {
            this.Parametro = nomParametro;
            this.Valor = valParametro;
            this.Tamano = tamanoParametro;
        }
        #endregion

    }
}