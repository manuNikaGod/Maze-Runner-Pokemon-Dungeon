using System;
using System.Collections.Generic;
using Spectre.Console;

/// <summary>
/// Representa una ficha en el juego.
/// </summary>
public class Ficha
{
    /// <summary>
    /// Nombre de la ficha.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Coordenada X de la ficha.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Coordenada Y de la ficha.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Velocidad de la ficha.
    /// </summary>
    public int Speed { get; set; }

    /// <summary>
    /// Habilidad predefinida de la ficha.
    /// </summary>
    public string HabilidadPredefinida { get; set; }

    /// <summary>
    /// Indica si la ficha fue teletransportada recientemente.
    /// </summary>
    public bool justTeleported { get; set; } = false;

    /// <summary>
    /// Velocidad inicial de la ficha, solo se puede establecer una vez.
    /// </summary>
    public int InitialSpeed { get; private set; }

    /// <summary>
    /// Mensaje de trampa asociado a la ficha.
    /// </summary>
    public string TrampaMensaje { get; set; }

    /// <summary>
    /// Diccionario de habilidades con su tiempo de enfriamiento.
    /// </summary>
    public Dictionary<string, int> Habilidades { get; set; } // Habilidad y su tiempo de enfriamiento

    /// <summary>
    /// Diccionario de habilidades en enfriamiento y su tiempo restante.
    /// </summary>
    public Dictionary<string, int> Cooldowns { get; set; } // Tiempo restante para que la habilidad esté disponible

    /// <summary>
    /// Diccionario de duraciones de efectos temporales.
    /// </summary>
    public Dictionary<string, int> Duraciones { get; set; } // Duraciones de efectos temporales


    /// <summary>
    /// Inicializa una nueva instancia de la clase Ficha.
    /// </summary>
    /// <param name="name">Nombre de la ficha.</param>
    /// <param name="x">Coordenada X de la ficha.</param>
    /// <param name="y">Coordenada Y de la ficha.</param>
    /// <param name="speed">Velocidad de la ficha.</param>
    /// <param name="habilidadPredefinida">Habilidad predefinida de la ficha.</param>
    public Ficha(string name, int x, int y, int speed, string habilidadPredefinida)
    {
        // Asigna el nombre de la ficha
        Name = name;
        // Asigna la coordenada X de la ficha
        X = x;
        // Asigna la coordenada Y de la ficha
        Y = y;
        // Asigna la velocidad de la ficha
        Speed = speed;
        // Asigna la habilidad predefinida de la ficha
        HabilidadPredefinida = habilidadPredefinida;
        // Asigna la velocidad inicial de la ficha
        InitialSpeed = speed;

        // Inicializa el diccionario de Cooldowns con la habilidad predefinida
        Cooldowns = new Dictionary<string, int>
    {
        { habilidadPredefinida, 0 }
    };

        // Inicializa el diccionario de Duraciones
        Duraciones = new Dictionary<string, int>();

        // Inicializa el diccionario de Habilidades
        Habilidades = new Dictionary<string, int>
    {
        { "Agilidad", 5 },
        { "Ventisca", 3 },
        { "Lullaby", 6 },
        { "Sombra Trampa", 5 },
        { "Ignorante", 2 },
        { "Oversoul", 5 },
        { "Don Aural", 7 },
        { "Teletransporte", 1 }
    };

        // Inicializa el diccionario de Cooldowns con valores por defecto
        Cooldowns = new Dictionary<string, int>
    {
        { "Agilidad", 0 },
        { "Ventisca", 0 },
        { "Lullaby", 0 },
        { "Sombra Trampa", 0 },
        { "Ignorante", 0 },
        { "Oversoul", 0 },
        { "Don Aural", 0 },
        { "Teletransporte", 0 }
    };

        // Inicializa el diccionario de Duraciones con valores por defecto
        Duraciones = new Dictionary<string, int>
    {
        { "Ventisca", 0 },
        { "Lullaby", 0 },
        { "Petrificacion", 0 },
        { "Ignorante", 0 },
        { "Oversoul", 0 },
        { "Don aural", 0 }
    };
    }

    /// <summary>
    /// Mueve la ficha a una nueva posición.
    /// </summary>
    /// <param name="deltaX">Cambio en la coordenada X.</param>
    /// <param name="deltaY">Cambio en la coordenada Y.</param>
    /// <param name="maze">El laberinto en el que se mueve la ficha.</param>
    /// <param name="teletransportado">Referencia a un indicador de teletransportación.</param>
    public void Move(int deltaX, int deltaY, int[,] maze, ref bool teletransportado)
    {
        int newX = X + deltaX; // Calcula la nueva coordenada X
        int newY = Y + deltaY; // Calcula la nueva coordenada Y

        // Comprobar si la nueva posición es válida
        if (newX >= 0 && newY >= 0 && newX < maze.GetLength(1) && newY < maze.GetLength(0))
        {
            // Verificar que la nueva posición sea un camino válido
            if (maze[newY, newX] == Maze.PATH || maze[newY, newX] == Maze.TRAP_SLOW || maze[newY, newX] == Maze.TRAP_CONFUSION || maze[newY, newX] == Maze.TRAP_TELEPORT)
            {
                // Si la ficha ya fue teletransportada, no se teletransporta de nuevo
                if (teletransportado && maze[newY, newX] == Maze.TRAP_TELEPORT)
                {
                    Console.WriteLine($"La ficha {Name} ya ha sido teletransportada y no se teletransportará nuevamente.");
                    return; // Terminar la función para evitar otra teletransportación
                }

                // Actualizar la posición de la ficha
                X = newX;
                Y = newY;
                Console.WriteLine($"Ficha movida a ({X}, {Y}).");

                // Efectos de las trampas
                if (!(Duraciones.ContainsKey("Ignorante") && Duraciones["Ignorante"] > 0)) // Verificar si la habilidad "Ignorante" está activa
                {
                    switch (maze[Y, X])
                    {
                        case Maze.TRAP_SLOW:
                            Console.WriteLine($"{Name} ha activado una trampa de ralentización. Movimientos restantes reducidos.");
                            break;

                        case Maze.TRAP_CONFUSION:
                            Duraciones["Petrification"] = 3; // La ficha no se podrá mover durante 2 turnos
                            Console.WriteLine($"{Name} ha activado una trampa de petrificación. No podrá moverse durante 2 turnos.");
                            break;

                        case Maze.TRAP_TELEPORT:
                            Teleport(maze); // Teletransportar a una posición aleatoria válida
                            teletransportado = true; // Marcar que la ficha ha sido teletransportada
                            Console.WriteLine($"{Name} ha sido teletransportada a una nueva posición.");
                            break;

                        default:
                            Console.WriteLine($"{Name} se ha movido a una posición segura.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"{Name} se ha movido ignorando las trampas.");
                }
            }
            else
            {
                Console.WriteLine("Posición inválida. Intenta nuevamente.");
            }
        }
        else
        {
            Console.WriteLine("Posición inválida. Intenta nuevamente.");
        }
    }


    /// <summary>
    /// Teletransporta la ficha a una nueva posición válida aleatoria en el laberinto.
    /// </summary>
    /// <param name="maze">El laberinto en el que se mueve la ficha.</param>
    public void Teleport(int[,] maze)
    {
        // Inicializa un nuevo generador de números aleatorios
        Random rand = new Random();
        int newX, newY;

        // Encontrar una posición válida aleatoria
        do
        {
            // Genera nuevas coordenadas aleatorias dentro del laberinto
            newX = rand.Next(maze.GetLength(1));
            newY = rand.Next(maze.GetLength(0));
        } while (maze[newY, newX] != Maze.PATH); // Repite hasta encontrar una celda que sea un camino

        // Actualizar la posición de la ficha
        X = newX;
        Y = newY;

        // Marcar la ficha como recién teletransportada
        justTeleported = true;

        // Imprimir un mensaje indicando la nueva posición de la ficha
        Console.WriteLine($"{Name} ha sido teletransportado a una nueva posición válida ({X}, {Y}).");
    }





    /// <summary>
    /// Usa una habilidad de la ficha, aplicando sus efectos sobre los enemigos y el laberinto.
    /// </summary>
    /// <param name="habilidad">El nombre de la habilidad a usar.</param>
    /// <param name="enemigos">Lista de fichas enemigas.</param>
    /// <param name="maze">El laberinto en el que se mueve la ficha.</param>
    /// <param name="currentPlayer">El jugador actual que está usando la habilidad.</param>
    /// <param name="propiasFichas">Lista de fichas propias del jugador.</param>
    /// <param name="movimientosRestantes">Referencia a los movimientos restantes de la ficha.</param>
    /// <param name="dificultad">La dificultad actual del laberinto.</param>
    public void UseHabilidad(string habilidad, List<Ficha> enemigos, Maze maze, Player currentPlayer, List<Ficha> propiasFichas, ref int movimientosRestantes, string dificultad)
    {
        Console.WriteLine($"Usando habilidad {habilidad} de {Name}");

        // Verificar si la habilidad está disponible (no en enfriamiento)
        if (Cooldowns[habilidad] == 0)
        {
            switch (habilidad)
            {
                case "Agilidad":
                    movimientosRestantes += 3; // Aumentar movimientos restantes en 3
                    Console.WriteLine($"{Name} ha usado {habilidad}. Los movimientos restantes se han aumentado a {movimientosRestantes}.");
                    Cooldowns[habilidad] = 6; // Cooldown de 6 turnos
                    break;

                case "Ventisca":
                    AplicarVentisca(enemigos);
                    Cooldowns[habilidad] = 5; // Cooldown de 5 turnos
                    break;

                case "Lullaby":
                    DormirEnemigosAleatorios(enemigos, 2); // Dormir a dos enemigos por dos turnos
                    Cooldowns[habilidad] = 7; // Cooldown de 7 turnos
                    break;

                case "Sombra Trampa":
                    maze.maze[Y, X] = Maze.TRAP_SLOW; // Crear una trampa de ralentización en el piso
                    Console.WriteLine($"{Name} ha usado {habilidad}, creando una trampa de ralentización en ({X}, {Y}).");
                    Cooldowns[habilidad] = 5; // Cooldown de 5 turnos
                    break;

                case "Ignorante":
                    Duraciones["Ignorante"] = 1; // La ficha no será afectada por trampas en este turno
                    Console.WriteLine($"{Name} ha usado {habilidad}, esta ficha no será afectada por trampas en este turno.");
                    Cooldowns[habilidad] = 7; // Cooldown de 7 turnos
                    break;

                case "Oversoul":
                    AplicarOversoul(enemigos);
                    Cooldowns[habilidad] = 8; // Cooldown de 8 turnos
                    break;

                case "Don Aural":
                    Duraciones["Don Aural"] = 3; // Lucario no será afectado por habilidades durante 3 turnos
                    Console.WriteLine($"{Name} ha usado {habilidad}. No será afectado por Ventisca, Lullaby ni Oversoul durante 3 turnos.");
                    Cooldowns[habilidad] = 7; // Cooldown de 7 turnos
                    break;

                case "Teletransporte":
                    Teleport(maze.maze); // Teletransportar a una posición aleatoria válida
                    Cooldowns[habilidad] = 1; // Cooldown de 1 turno

                    Console.WriteLine($"{Name} se ha teletransportado a una nueva posición.");
                    AnsiConsole.Clear();
                    maze.DisplayMaze(currentPlayer.Fichas, propiasFichas, currentPlayer.CurrentPlayerIndex, dificultad);
                    break;

                default:
                    Console.WriteLine("Habilidad desconocida.");
                    break;
            }
        }
        else
        {
            Console.WriteLine("La habilidad está en enfriamiento.");
        }
    }


    /// <summary>
    /// Verifica si una ficha es inmune debido a la habilidad "Don Aural".
    /// </summary>
    /// <param name="ficha">La ficha a verificar.</param>
    /// <returns>true si la ficha es inmune, de lo contrario, false.</returns>
    private bool EsInmuneAFicha(Ficha ficha)
    {
        // Verifica si la ficha tiene la habilidad "Don Aural" activa
        return ficha.Duraciones.ContainsKey("Don Aural") && ficha.Duraciones["Don Aural"] > 0;
    }

    /// <summary>
    /// Duerme una cantidad aleatoria de enemigos por un periodo de tiempo, ignorando aquellos con inmunidad "Don Aural".
    /// </summary>
    /// <param name="enemigos">Lista de fichas enemigas.</param>
    /// <param name="cantidad">Cantidad de enemigos a dormir.</param>
    private void DormirEnemigosAleatorios(List<Ficha> enemigos, int cantidad)
    {
        // Inicializa un nuevo generador de números aleatorios
        Random random = new Random();

        // Lista para almacenar los enemigos seleccionados
        List<Ficha> seleccionados = new List<Ficha>();

        // Seleccionar enemigos aleatorios hasta alcanzar la cantidad deseada o el número de enemigos disponibles
        while (seleccionados.Count < cantidad && seleccionados.Count < enemigos.Count)
        {
            // Generar un índice aleatorio para seleccionar un enemigo
            int index = random.Next(enemigos.Count);

            // Verificar si el enemigo ya ha sido seleccionado y si no es inmune
            if (!seleccionados.Contains(enemigos[index]) && !EsInmuneAFicha(enemigos[index]))
            {
                // Añadir el enemigo a la lista de seleccionados
                seleccionados.Add(enemigos[index]);

                // Establecer la duración del efecto de Lullaby a 3 turnos
                enemigos[index].Duraciones["Lullaby"] = 3; // Dormir enemigo por 3 turnos
                Console.WriteLine($"{enemigos[index].Name} ha sido dormido por Lullaby durante 3 turnos.");
            }
            else if (EsInmuneAFicha(enemigos[index]))
            {
                Console.WriteLine($"{enemigos[index].Name} es inmune a Lullaby debido a Don Aural.");
            }
        }
    }

    /// <summary>
    /// Convierte la velocidad de todas las fichas enemigas en 1, ignorando a aquellos con la habilidad Don Aural.
    /// </summary>
    /// <param name="enemigos">Lista de fichas enemigas.</param>
    private void AplicarOversoul(List<Ficha> enemigos)
    {
        foreach (var enemigo in enemigos)
        {
            // Verifica si el enemigo es inmune a Oversoul debido a Don Aural
            if (EsInmuneAFicha(enemigo))
            {
                Console.WriteLine($"{enemigo.Name} es inmune a Oversoul debido a Don Aural.");
                continue;
            }

            // No es necesario almacenar la velocidad original, ya que usamos InitialSpeed
            enemigo.Speed = 1; // Reducir temporalmente la velocidad a 1
            enemigo.Duraciones["Oversoul"] = 3; // Duración de 3 turnos
            Console.WriteLine($"La velocidad de {enemigo.Name} se ha reducido a 1 por la habilidad Oversoul.");
        }
        Cooldowns["Oversoul"] = 5; // Establece el tiempo de recarga del método Oversoul en 5 turnos
    }
    /// <summary>
    /// Aplica el efecto de la habilidad Ventisca a los enemigos, congelándolos por un turno.
    /// </summary>
    /// <param name="enemigos">Lista de fichas enemigas.</param>
    private void AplicarVentisca(List<Ficha> enemigos)
    {
        // Itera sobre cada enemigo en la lista de enemigos
        foreach (var enemigo in enemigos)
        {
            // Verifica si el enemigo es inmune a la habilidad
            if (EsInmuneAFicha(enemigo))
            {
                // Imprime un mensaje si el enemigo es inmune debido a "Don Aural"
                Console.WriteLine($"{enemigo.Name} es inmune a Ventisca debido a Don Aural.");
                // Continúa con el siguiente enemigo
                continue;
            }

            // Establece la duración del efecto "Ventisca" en 1 turno, congelando al enemigo
            enemigo.Duraciones["Ventisca"] = 1; // Congelar a los enemigos por 1 turno
                                                // Imprime un mensaje indicando que el enemigo ha sido congelado
            Console.WriteLine($"{enemigo.Name} ha sido congelado por Ventisca durante 1 turno.");
        }

        // Establece el tiempo de recarga del método "Ventisca" en 3 turnos
        Cooldowns["Ventisca"] = 3;
    }







}
