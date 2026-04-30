namespace ApiMovimientosSociales
{
    public class Registro
    {
        public int id { get; set; }
        public string tipo_registro { get; set; } = "";
        public string tipo_apoyo { get; set; } = "";
        public string nombres { get; set; } = "";
        public string apellidos { get; set; } = "";
        public string dui { get; set; } = "";
        public string telefono { get; set; } = "";
        public string cargo { get; set; } = "";
        public string comunidad { get; set; } = "";
        public string departamento { get; set; } = "";
        public string distrito { get; set; } = "";
        public DateTime? fecha { get; set; }
        public string creado_por { get; set; } = "";
    }
}