using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace TempoControl
{
    // ===================== Dominio =====================
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Departamento { get; set; }
        public string Posicion { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class RegistroFichaje
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime HoraEntrada { get; set; }
        public DateTime HoraSalida { get; set; }
    }

    // ===================== Interfaces =====================
    public interface IEmpleadoRepository
    {
        void Crear(Empleado e);
        List<Empleado> ObtenerTodos();
        Empleado ObtenerPorId(int id);
        void Actualizar(Empleado e);
        void Desactivar(int id);
    }

    public interface IRegistroFichajeRepository
    {
        void RegistrarEntrada(int empleadoId);
        void RegistrarSalida(int empleadoId);
        List<RegistroFichaje> ObtenerPorEmpleado(int empleadoId, int mes, int anio);
    }

    // ===================== Repositorios =====================
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly string connectionString = "Data Source=TempoControl.db";

        public EmpleadoRepository()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Empleados(
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Nombre TEXT, Departamento TEXT, Posicion TEXT, Activo INTEGER)";
            cmd.ExecuteNonQuery();
        }

        public void Crear(Empleado e)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Empleados(Nombre,Departamento,Posicion,Activo) VALUES(@n,@d,@p,1)";
            cmd.Parameters.AddWithValue("@n", e.Nombre);
            cmd.Parameters.AddWithValue("@d", e.Departamento);
            cmd.Parameters.AddWithValue("@p", e.Posicion);
            cmd.ExecuteNonQuery();
        }

        public List<Empleado> ObtenerTodos()
        {
            var lista = new List<Empleado>();
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Empleados";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Empleado
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString(),
                    Departamento = reader["Departamento"].ToString(),
                    Posicion = reader["Posicion"].ToString(),
                    Activo = Convert.ToInt32(reader["Activo"]) == 1
                });
            }
            return lista;
        }

        public Empleado ObtenerPorId(int id)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Empleados WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Empleado
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString(),
                    Departamento = reader["Departamento"].ToString(),
                    Posicion = reader["Posicion"].ToString(),
                    Activo = Convert.ToInt32(reader["Activo"]) == 1
                };
            }
            return null;
        }

        public void Actualizar(Empleado e)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Empleados SET Nombre=@n,Departamento=@d,Posicion=@p WHERE Id=@id";
            cmd.Parameters.AddWithValue("@n", e.Nombre);
            cmd.Parameters.AddWithValue("@d", e.Departamento);
            cmd.Parameters.AddWithValue("@p", e.Posicion);
            cmd.Parameters.AddWithValue("@id", e.Id);
            cmd.ExecuteNonQuery();
        }

        public void Desactivar(int id)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Empleados SET Activo=0 WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public class RegistroFichajeRepository : IRegistroFichajeRepository
    {
        private readonly string connectionString = "Data Source=TempoControl.db";

        public RegistroFichajeRepository()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Registros(
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                EmpleadoId INTEGER, HoraEntrada TEXT, HoraSalida TEXT)";
            cmd.ExecuteNonQuery();
        }

        public void RegistrarEntrada(int empleadoId)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Registros(EmpleadoId,HoraEntrada) VALUES(@id,@h)";
            cmd.Parameters.AddWithValue("@id", empleadoId);
            cmd.Parameters.AddWithValue("@h", DateTime.Now.ToString("s"));
            cmd.ExecuteNonQuery();
        }

        public void RegistrarSalida(int empleadoId)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Registros SET HoraSalida=@h WHERE EmpleadoId=@id AND HoraSalida IS NULL";
            cmd.Parameters.AddWithValue("@id", empleadoId);
            cmd.Parameters.AddWithValue("@h", DateTime.Now.ToString("s"));
            cmd.ExecuteNonQuery();
        }

        public List<RegistroFichaje> ObtenerPorEmpleado(int empleadoId, int mes, int anio)
        {
            var lista = new List<RegistroFichaje>();
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Registros WHERE EmpleadoId=@id";
            cmd.Parameters.AddWithValue("@id", empleadoId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var entrada = DateTime.Parse(reader["HoraEntrada"].ToString());
                if (entrada.Month == mes && entrada.Year == anio)
                {
                    lista.Add(new RegistroFichaje
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        EmpleadoId = Convert.ToInt32(reader["EmpleadoId"]),
                        HoraEntrada = entrada,
                        HoraSalida = string.IsNullOrEmpty(reader["HoraSalida"].ToString()) ? DateTime.MinValue : DateTime.Parse(reader["HoraSalida"].ToString())
                    });
                }
            }
            return lista;
        }
    }

    // ===================== Lógica de Negocio =====================
    public class GestorReportes
    {
        private readonly IEmpleadoRepository empleadoRepo;
        private readonly IRegistroFichajeRepository registroRepo;

        public GestorReportes(IEmpleadoRepository eRepo, IRegistroFichajeRepository rRepo)
        {
            empleadoRepo = eRepo;
            registroRepo = rRepo;
        }

        public void GenerarReporteMensual(int mes, int anio)
        {
            var empleados = empleadoRepo.ObtenerTodos();
            Console.WriteLine($"Reporte Mensual {mes}/{anio}");
            Console.WriteLine("=====================================");
            foreach (var emp in empleados.Where(e => e.Activo))
            {
                var registros = registroRepo.ObtenerPorEmpleado(emp.Id, mes, anio);
                int dias = registros.Count(r => r.HoraSalida != DateTime.MinValue);
                double horas = registros.Sum(r => (r.HoraSalida - r.HoraEntrada).TotalHours);
                Console.WriteLine($"{emp.Nombre} - Días: {dias}, Horas: {horas:F2}");
            }
        }
    }

    // ===================== Presentación =====================
    class Program
    {
        static void Main()
        {
            var empleadoRepo = new EmpleadoRepository();
            var registroRepo = new RegistroFichajeRepository();
            var gestor = new GestorReportes(empleadoRepo, registroRepo);

            while (true)
            {
                Console.WriteLine("\n--- TempoControl ---");
                Console.WriteLine("1. Crear Empleado");
                Console.WriteLine("2. Listar Empleados");
                Console.WriteLine("3. Actualizar Empleado");
                Console.WriteLine("4. Desactivar Empleado");
                Console.WriteLine("5. Registrar Entrada");
                Console.WriteLine("6. Registrar Salida");
                Console.WriteLine("7. Generar Reporte Mensual");
                Console.WriteLine("0. Salir");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        Console.Write("Nombre: ");
                        string nombre = Console.ReadLine();
                        Console.Write("Departamento: ");
                        string depto = Console.ReadLine();
                        Console.Write("Posición: ");
                        string pos = Console.ReadLine();
                        empleadoRepo.Crear(new Empleado { Nombre = nombre, Departamento = depto, Posicion = pos });
                        Console.WriteLine("Empleado creado.");
                        break;

                    case "2":
                        var empleados = empleadoRepo.ObtenerTodos();
                        foreach (var e in empleados)
                            Console.WriteLine($"{e.Id} - {e.Nombre} ({e.Departamento}, {e.Posicion}) Activo:{e.Activo}");
                        break;

                    case "3":
                        Console.Write("ID del empleado a actualizar: ");
                        int idUp = int.Parse(Console.ReadLine());
                        var empUp = empleadoRepo.ObtenerPorId(idUp);
                        if (empUp != null)
                        {
                            Console.Write("Nuevo Nombre: ");
                            empUp.Nombre = Console.ReadLine();
                            Console.Write("Nuevo Departamento: ");
                            empUp.Departamento = Console.ReadLine();
                            Console.Write("Nueva Posición: ");
                            empUp.Posicion = Console.ReadLine();
                            empleadoRepo.Actualizar(empUp);
                            Console.WriteLine("Empleado actualizado.");
                        }
                        break;

                    case "4":
                        Console.Write("ID del empleado a desactivar: ");
                        int idDes = int.Parse(Console.ReadLine());
                        empleadoRepo.Desactivar(idDes);
                        Console.WriteLine("Empleado desactivado.");
                        break;

                    case "5":
                        Console.Write("ID del empleado: ");
                        int idEnt = int.Parse(Console.ReadLine());
                        registroRepo.RegistrarEntrada(idEnt);
                        Console.WriteLine("Entrada registrada.");
                        break;

                    case "6":
                        Console.Write("ID del empleado: ");
                        int idSal = int.Parse(Console.ReadLine());
                        registroRepo.RegistrarSalida(idSal);
                        Console.WriteLine("Salida registrada.");
                        break;

                    case "7":
                        Console.Write("Mes (1-12): ");
                        int mes = int.Parse(Console.ReadLine());
                        Console.Write("Año: ");
                        int anio = int.Parse(Console.ReadLine());
                        gestor.GenerarReporteMensual(mes, anio);
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }
        }
    }
}
u