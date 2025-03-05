 using System;
using System.Collections.Generic;
using Spectre.Console;
using System.Text;

/// <summary>
/// Representa un laberinto.
/// </summary>
public class Maze
{
    /// <summary>
    /// Obtiene el ancho del laberinto.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Obtiene la altura del laberinto.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Representa el laberinto.
    /// </summary>
    public int[,] maze;

    /// <summary>
    /// Generador de números aleatorios.
    /// </summary>
    private Random rand = new Random();

    /// <summary>
    /// Valor constante que representa una pared.
    /// </summary>
    public const int WALL = 1;

    /// <summary>
    /// Valor constante que representa un camino.
    /// </summary>
    public const int PATH = 0;

    /// <summary>
    /// Valor constante que representa una trampa de ralentización.
    /// </summary>
    public const int TRAP_SLOW = 3;

    /// <summary>
    /// Valor constante que representa una trampa de confusión.
    /// </summary>
    public const int TRAP_CONFUSION = 4;

    /// <summary>
    /// Valor constante que representa una trampa de teletransportación.
    /// </summary>
    public const int TRAP_TELEPORT = 5;




    /// <summary>
    /// Inicializa una nueva instancia de la clase Maze con dimensiones especificadas.
    /// </summary>
    /// <param name="width">El ancho del laberinto.</param>
    /// <param name="height">La altura del laberinto.</param>
    public Maze(int width = 15, int height = 15)
    {
        // Asigna el ancho del laberinto al valor proporcionado en el parámetro 'width'
        this.Width = width;

        // Asigna la altura del laberinto al valor proporcionado en el parámetro 'height'
        this.Height = height;

        // Inicializa la matriz del laberinto con las dimensiones especificadas de ancho y alto
        this.maze = new int[height, width];
    }



    /// <summary>
    /// Genera el laberinto utilizando un algoritmo de crecimiento basado en fronteras.
    /// </summary>
    public void GenerateMaze()
    {
        // Inicializa todas las casillas como paredes
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                maze[y, x] = WALL; // Marca la celda como pared
            }
        }

        // Punto de inicio del laberinto
        var startX = 1;
        var startY = 1;
        maze[startY, startX] = PATH; // Marca el punto de inicio como camino

        var frontier = new List<(int x, int y)>();
        AddFrontier(startX, startY, frontier); // Añade las fronteras iniciales

        // Algoritmo de generación de laberinto
        while (frontier.Count > 0)
        {
            var current = frontier[rand.Next(frontier.Count)]; // Selecciona una celda aleatoria de la frontera
            frontier.Remove(current); // Elimina la celda seleccionada de la frontera

            var neighbors = GetNeighbors(current.x, current.y); // Obtiene los vecinos válidos de la celda seleccionada
            if (neighbors.Count > 0)
            {
                var neighbor = neighbors[rand.Next(neighbors.Count)]; // Selecciona un vecino aleatorio
                maze[current.y, current.x] = PATH; // Marca la celda actual como camino
                maze[(current.y + neighbor.y) / 2, (current.x + neighbor.x) / 2] = PATH; // Marca la celda intermedia como camino
                AddFrontier(current.x, current.y, frontier); // Añade las nuevas fronteras

                // Asegurarse de que hay al menos una vuelta en el camino
                if ((current.x == startX && current.y == startY + 1) || (current.x == startX + 1 && current.y == startY))
                {
                    var next = frontier[rand.Next(frontier.Count)]; // Selecciona una celda adicional de la frontera
                    maze[next.y, next.x] = PATH; // Marca la celda adicional como camino
                    maze[(next.y + neighbor.y) / 2, (next.x + neighbor.x) / 2] = PATH; // Marca la celda intermedia como camino
                    AddFrontier(next.x, next.y, frontier); // Añade las nuevas fronteras
                }
            }
        }

        // Asegurarse de que la casilla final no esté bloqueada por trampas
        maze[Height - 2, Width - 2] = PATH; // Marca la celda final como camino

        BlockLastRowExceptExit(); // Bloquea la última fila excepto la salida
        PlaceRandomObstaclesAndTraps(10, (Height - 2, Width - 2)); // Asegura que las trampas no se coloquen en la casilla final
        MakeEdgesInaccessible(); // Hace inaccesibles los bordes del laberinto
        AsegurarMetaDesbloqueada();//Hace q nunca una trampa de teletransportacion bloquee la salida
    }

    /// <summary>
    /// Coloca obstáculos y trampas aleatoriamente en el laberinto.
    /// </summary>
    /// <param name="count">El número de trampas a colocar.</param>
    /// <param name="exit">La coordenada de la salida del laberinto.</param>
    private void PlaceRandomObstaclesAndTraps(int count, (int y, int x) exit)
    {

        // Lista para almacenar las posiciones libres donde se pueden colocar trampas
        List<(int, int)> posicionesLibres = new List<(int, int)>();

        // Añadir todas las posiciones libres a la lista
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                // Si la celda es un camino y no es la salida, se añade a la lista de posiciones libres
                if (maze[y, x] == PATH && !(y == exit.y && x == exit.x))
                {
                    posicionesLibres.Add((x, y));
                }
            }
        }

        // Método auxiliar para colocar trampas evitando la entrada y salida
        void ColocarTrampas(int tipoTrampa, int cantidad, (int y, int x) entrada, (int y, int x) salida)
        {
            for (int i = 0; i < cantidad; i++)
            {
                // Si no hay posiciones libres restantes, se rompe el ciclo
                if (posicionesLibres.Count == 0)
                {
                    break;
                }

                int index;
                (int x, int y) posicion;
                do
                {
                    // Selecciona una posición aleatoria de la lista de posiciones libres
                    index = rand.Next(posicionesLibres.Count);
                    posicion = posicionesLibres[index];
                } while ((posicion == entrada || posicion == salida) ||
                         (posicion == (entrada.y + 1, entrada.x)) || 
                         (posicion == (salida.x - 1, salida.y)) ||
                         (posicion == (salida.x + 1, salida.y)));

                // Coloca la trampa en la posición seleccionada
                maze[posicion.y, posicion.x] = tipoTrampa;

                // Elimina la posición seleccionada de la lista de posiciones libres
                posicionesLibres.RemoveAt(index);
            }
        }

        // Colocar trampas asegurándose de que no están frente a la entrada y salida
        ColocarTrampas(TRAP_SLOW, 3, (1, 1), exit);
        ColocarTrampas(TRAP_CONFUSION, 3, (1, 1), exit);
        ColocarTrampas(TRAP_TELEPORT, 3, (1, 1), exit);
    }



    /// <summary>
    /// Añade posiciones de frontera para el algoritmo de generación del laberinto.
    /// </summary>
    /// <param name="x">La coordenada x de la posición.</param>
    /// <param name="y">La coordenada y de la posición.</param>
    /// <param name="frontier">La lista de posiciones de frontera.</param>
    private void AddFrontier(int x, int y, List<(int x, int y)> frontier)
    {
        // Añade la posición a la izquierda si está dentro de los límites y es una pared
        if (x >= 2 && maze[y, x - 2] == WALL)
        {
            frontier.Add((x - 2, y));
        }

        // Añade la posición arriba si está dentro de los límites y es una pared
        if (y >= 2 && maze[y - 2, x] == WALL)
        {
            frontier.Add((x, y - 2));
        }

        // Añade la posición a la derecha si está dentro de los límites y es una pared
        if (x < Width - 2 && maze[y, x + 2] == WALL)
        {
            frontier.Add((x + 2, y));
        }

        // Añade la posición abajo si está dentro de los límites y es una pared
        if (y < Height - 2 && maze[y + 2, x] == WALL)
        {
            frontier.Add((x, y + 2));
        }
    }


    /// <summary>
    /// Obtiene los vecinos válidos de una posición en el laberinto.
    /// </summary>
    /// <param name="x">La coordenada x de la posición.</param>
    /// <param name="y">La coordenada y de la posición.</param>
    /// <returns>Una lista de vecinos válidos.</returns>
    private List<(int x, int y)> GetNeighbors(int x, int y)
    {
        // Lista para almacenar los vecinos válidos
        var neighbors = new List<(int x, int y)>();

        // Añade la posición a la izquierda si está dentro de los límites y es un camino
        if (x >= 2 && maze[y, x - 2] == PATH)
        {
            neighbors.Add((x - 2, y));
        }

        // Añade la posición arriba si está dentro de los límites y es un camino
        if (y >= 2 && maze[y - 2, x] == PATH)
        {
            neighbors.Add((x, y - 2));
        }

        // Añade la posición a la derecha si está dentro de los límites y es un camino
        if (x < Width - 2 && maze[y, x + 2] == PATH)
        {
            neighbors.Add((x + 2, y));
        }

        // Añade la posición abajo si está dentro de los límites y es un camino
        if (y < Height - 2 && maze[y + 2, x] == PATH)
        {
            neighbors.Add((x, y + 2));
        }

        // Retorna la lista de vecinos válidos
        return neighbors;
    }


    /// <summary>
    /// Hace que todos los bordes del laberinto sean inaccesibles excepto la entrada y la salida.
    /// </summary>
    public void MakeEdgesInaccessible()
    {
        // Borde superior: Marca todas las celdas en la fila superior como paredes
        for (int x = 0; x < Width; x++)
        {
            maze[0, x] = WALL;
        }

        // Borde inferior: Marca todas las celdas en la fila inferior como paredes
        for (int x = 0; x < Width; x++)
        {
            maze[Height - 1, x] = WALL;
        }

        // Borde izquierdo: Marca todas las celdas en la columna izquierda como paredes
        for (int y = 0; y < Height; y++)
        {
            maze[y, 0] = WALL;
        }

        // Borde derecho: Marca todas las celdas en la columna derecha como paredes
        for (int y = 0; y < Height; y++)
        {
            maze[y, Width - 1] = WALL;
        }

        // Mantener la entrada accesible: Marca la celda (1, 1) como camino
        maze[1, 1] = PATH;

        // Mantener la salida accesible: Marca la celda (Height - 2, Width - 2) como camino
        maze[Height - 2, Width - 2] = PATH;
    }


    /// <summary>
    /// Bloquea la última fila excepto la salida.
    /// </summary>
    private void BlockLastRowExceptExit()
    {
        // Itera sobre todas las celdas en la última fila excepto la última celda
        for (int j = 0; j < Width - 1; j++)
        {
            maze[Height - 1, j] = WALL; // Marca cada celda en la última fila como pared
        }

        // Asegurarse de que la salida esté abierta: Marca la celda (Height - 2, Width - 2) como camino
        maze[Height - 2, Width - 2] = PATH;
    }


    /// <summary>
    /// Coloca obstáculos y trampas aleatoriamente en el laberinto.
    /// </summary>
    /// <param name="numTraps">El número de trampas a colocar.</param>
    public void PlaceRandomObstaclesAndTraps(int numTraps)
    {
        for (int i = 0; i < numTraps; i++)
        {
            // Colocar trampas en el modo normal
            if (numTraps == 9)
            {
                PlaceRandomObject(TRAP_SLOW);
                PlaceRandomObject(TRAP_CONFUSION);
                PlaceRandomObject(TRAP_TELEPORT);
            }
            // Colocar trampas en el modo pesadilla
            else if (numTraps == 30)
            {
                PlaceRandomObject(TRAP_SLOW);
                PlaceRandomObject(TRAP_CONFUSION);
                PlaceRandomObject(TRAP_TELEPORT);
            }
        }
    }


    /// <summary>
    /// Coloca un objeto aleatorio en una posición válida del laberinto.
    /// </summary>
    /// <param name="objectType">El tipo de objeto a colocar.</param>
    private void PlaceRandomObject(int objectType)
    {
        // Inicializar la instancia de Random
        Random random = new Random();

        int x, y;
        do
        {
            // Genera coordenadas aleatorias dentro de los límites del laberinto
            x = random.Next(1, Width - 1);
            y = random.Next(1, Height - 1);
        } while (maze[y, x] != PATH || (x == 1 && y == 1) || (x == Width - 2 && y == Height - 2)); // Asegurarse de no bloquear la entrada ni la salida

        // Coloca el objeto en la posición seleccionada
        maze[y, x] = objectType;
    }

    /// <summary>
    /// Asegura que las trampas de teletransportación no bloqueen la casilla de la meta.
    /// </summary>
    public void AsegurarMetaDesbloqueada()
    {
        int exitX = Width - 2; // Coordenada X de la meta
        int exitY = Height - 2; // Coordenada Y de la meta

        // Verificar si la casilla de la meta está bloqueada por una trampa de teletransportación
        if (maze[exitY, exitX] == TRAP_TELEPORT)
        {
            // Remover la trampa de teletransportación de la casilla de la meta
            maze[exitY, exitX] = PATH;
        }

        // Verificar si hay un solo camino para llegar a la meta
        int validPaths = 0; // Contador de caminos válidos
        if (maze[exitY - 1, exitX] == PATH) validPaths++; // Camino desde arriba
        if (maze[exitY + 1, exitX] == PATH) validPaths++; // Camino desde abajo
        if (maze[exitY, exitX - 1] == PATH) validPaths++; // Camino desde la izquierda
        if (maze[exitY, exitX + 1] == PATH) validPaths++; // Camino desde la derecha

        // Si hay un solo camino hacia la meta, asegurar que las trampas no bloqueen las celdas adyacentes
        if (validPaths == 1)
        {
            // Verificar las celdas adyacentes a la casilla de la meta
            if (maze[exitY - 1, exitX] == TRAP_TELEPORT) maze[exitY - 1, exitX] = PATH; // Quitar trampa desde arriba
            if (maze[exitY + 1, exitX] == TRAP_TELEPORT) maze[exitY + 1, exitX] = PATH; // Quitar trampa desde abajo
            if (maze[exitY, exitX - 1] == TRAP_TELEPORT) maze[exitY, exitX - 1] = PATH; // Quitar trampa desde la izquierda
            if (maze[exitY, exitX + 1] == TRAP_TELEPORT) maze[exitY, exitX + 1] = PATH; // Quitar trampa desde la derecha
        }
    }


    /// <summary>
    /// Muestra el laberinto en la consola usando AnsiConsole.
    /// </summary>
    /// <param name="player1Fichas">Lista de fichas del Jugador 1.</param>
    /// <param name="player2Fichas">Lista de fichas del Jugador 2.</param>
    /// <param name="currentPlayerIndex">Índice del jugador actual.</param>
    /// <param name="dificultad">La dificultad del laberinto.</param>
    public void DisplayMaze(List<Ficha> player1Fichas, List<Ficha> player2Fichas, int currentPlayerIndex, string dificultad)
    {
        // Colores para los números
        string[] colors = { "red", "green", "yellow", "blue", "magenta", "cyan", "white", "gray" };

        // Limpia la consola
        AnsiConsole.Clear();
        // Inicializa un StringBuilder para construir la representación del laberinto
        var sb = new StringBuilder();

        // Añadir números superiores para la identificación de las columnas
        sb.Append("   ");
        for (int x = 0; x < Width; x++)
        {
            sb.Append($"[bold {colors[x % colors.Length]}]{x:D2}[/]"); // Números horizontales con dos dígitos
        }
        sb.AppendLine();

        // Añadir filas y contenido del laberinto
        for (int y = 0; y < Height; y++)
        {
            // Añadir número de fila en el lado derecho
            sb.Append($"[bold {colors[y % colors.Length]}]{y:D2}[/] ");
            for (int x = 0; x < Width; x++)
            {
                if (y == 1 && x == 1)
                {
                    sb.Append("[blue]🚪[/]"); // Representa el punto de inicio
                }
                else if (y == Height - 2 && x == Width - 2)
                {
                    sb.Append("[yellow]🏁[/]"); // Representa la salida
                }
                else
                {
                    bool fichaEncontrada = false;

                    // Verificar si hay una ficha del Jugador 1
                    foreach (var ficha in player1Fichas)
                    {
                        if (ficha.X == x && ficha.Y == y)
                        {
                            sb.Append($"[magenta]{GetPokemonEmoji(ficha.Name)}[/]"); // Representa una ficha del Jugador 1
                            fichaEncontrada = true;
                            break;
                        }
                    }

                    // Verificar si hay una ficha del Jugador 2 si no se encontró una ficha del Jugador 1
                    if (!fichaEncontrada)
                    {
                        foreach (var ficha in player2Fichas)
                        {
                            if (ficha.X == x && ficha.Y == y)
                            {
                                sb.Append($"[white]{GetPokemonEmoji(ficha.Name)}[/]"); // Representa una ficha del Jugador 2
                                fichaEncontrada = true;
                                break;
                            }
                        }
                    }

                    // Si no se encontró ninguna ficha, mostrar el contenido del laberinto
                    if (!fichaEncontrada)
                    {
                        if (dificultad == "Tutorial")
                        {
                            switch (maze[y, x])
                            {
                                case WALL:
                                    sb.Append("[gray]🪨[/]"); // Mostrar la pared
                                    break;
                                case PATH:
                                    sb.Append("[green]▒▒[/]"); // Mostrar el camino
                                    break;
                                case TRAP_SLOW:
                                    sb.Append("[red]🐢[/]"); // Mostrar la trampa de ralentización
                                    break;
                                case TRAP_CONFUSION:
                                    sb.Append("[yellow]💫[/]"); // Mostrar la trampa de confusión
                                    break;
                                case TRAP_TELEPORT:
                                    sb.Append("[purple]🌀[/]"); // Mostrar la trampa de teletransportación
                                    break;
                            }
                        }
                        else if (dificultad == "Normal")
                        {
                            switch (maze[y, x])
                            {
                                case WALL:
                                    sb.Append("[green]🌳[/]"); // Mostrar las paredes como árboles
                                    break;
                                case PATH:
                                    sb.Append("[green]▒▒[/]"); // Mostrar el camino en verde
                                    break;
                                case TRAP_SLOW:
                                case TRAP_CONFUSION:
                                case TRAP_TELEPORT:
                                    sb.Append("[green]▒▒[/]"); // Mostrar las trampas como el camino
                                    break;
                            }
                        }
                        else if (dificultad == "Pesadilla")
                        {
                            switch (maze[y, x])
                            {
                                case WALL:
                                    sb.Append("[gray]🕸️[/]"); // Mostrar las paredes como telarañas
                                    break;
                                case PATH:
                                    sb.Append("[purple]▒▒[/]"); // Mostrar el camino en morado
                                    break;
                                case TRAP_SLOW:
                                case TRAP_CONFUSION:
                                case TRAP_TELEPORT:
                                    sb.Append("[purple]▒▒[/]"); // Mostrar las trampas como el camino
                                    break;
                            }
                        }
                    }
                }
            }
            // Añadir número de fila en el lado derecho
            sb.Append($" [bold {colors[y % colors.Length]}]{y:D2}[/]");
            sb.AppendLine(); // Nueva línea después de cada fila
        }

        // Muestra el laberinto en la consola
        AnsiConsole.Markup(sb.ToString());
        // Restablece el color de fondo de la consola
        AnsiConsole.Reset();
    }

    /// <summary>
    /// Obtiene el emoji correspondiente a un Pokémon dado su nombre.
    /// </summary>
    /// <param name="pokemonName">El nombre del Pokémon.</param>
    /// <returns>El emoji correspondiente al Pokémon, o un signo de interrogación si no se encuentra.</returns>
    public string GetPokemonEmoji(string pokemonName)
    {
        // Convertir el nombre del Pokémon a minúsculas para hacer la comparación
        switch (pokemonName.ToLower())
        {
            case "beedrill":
                return "🐝"; // Emoji para Mega Beedrill
            case "sandshrew":
                return "🦔"; // Emoji para Sandshrew
            case "jigglypuff":
                return "🎤"; // Emoji para Jigglypuff
            case "gengar":
                return "👻"; // Emoji para Gengar
            case "snivy":
                return "🍃"; // Emoji para Snivy
            case "litwick":
                return "🕯️"; // Emoji para Litwick
            case "decidueye":
                return "🦉"; // Emoji para Decidueye
            case "litten":
                return "🐱"; // Emoji para Litten
            case "psyduck":
                return "🦆"; // Emoji para Psyduck
            case "greninja":
                return "🐸"; // Emoji para Greninja
            case "charmander":
                return "🔥"; // Emoji para Charmander
            case "bulbasaur":
                return "🌿"; // Emoji para Bulbasaur
            case "squirtle":
                return "💧"; // Emoji para Squirtle
            case "pikachu":
                return "⚡"; // Emoji para Pikachu
            case "eevee":
                return "🦊"; // Emoji para Eevee
            case "lucario":
                return "🧘"; // Emoji para Lucario
            case "abra":
                return "🥄"; // Emoji para Abra
            default:
                return "❓"; // Emoji para Pokémon desconocido
        }
    }

    /// <summary>
    /// Configura el laberinto del tutorial con trampas predefinidas en un tamaño 8x8.
    /// </summary>
    public void InitializeTutorialMaze()
    {
        // Configuración del laberinto del tutorial con trampas predefinidas en un tamaño 8x8
        maze = new int[,]
        {
        { WALL, WALL, WALL, WALL, WALL, WALL, WALL, WALL },
        { WALL, PATH, PATH, PATH, PATH, TRAP_CONFUSION, PATH, WALL },
        { WALL, PATH, PATH, TRAP_TELEPORT, WALL, PATH, TRAP_SLOW, WALL },
        { WALL, PATH, WALL, PATH, TRAP_SLOW, PATH, PATH, WALL },
        { WALL, PATH, TRAP_CONFUSION, PATH, WALL, TRAP_TELEPORT, PATH, WALL },
        { WALL, PATH, WALL, PATH, PATH, WALL, PATH, WALL },
        { WALL, PATH, TRAP_SLOW, PATH, TRAP_CONFUSION, PATH, TRAP_TELEPORT, WALL },
        { WALL, WALL, WALL, WALL, WALL, WALL, WALL, WALL }
        };
    }



}
