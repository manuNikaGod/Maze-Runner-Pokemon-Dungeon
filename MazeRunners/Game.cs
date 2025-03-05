using System;
using System.Collections.Generic;
using Spectre.Console;

namespace MazeRunners
{
    /// <summary>
    /// Representa el juego y gestiona la lógica del mismo.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Primer jugador.
        /// </summary>
        public Player Player1 { get; set; }

        /// <summary>
        /// Segundo jugador.
        /// </summary>
        public Player Player2 { get; set; }

        /// <summary>
        /// Representa el laberinto.
        /// </summary>
        public Maze Maze { get; set; }

        /// <summary>
        /// Cantidad de fichas que usarán ambos jugadores.
        /// </summary>
        public int NumFichas { get; set; }

        /// <summary>
        /// Índice del jugador que tiene el turno actual.
        /// </summary>
        public int currentPlayerIndex = 1;

        /// <summary>
        /// Lista de fichas disponibles para la selección.
        /// </summary>
        public List<Ficha> AvailableFicha { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase Game.
        /// </summary>
        private MusicPlayer musicPlayer = new MusicPlayer();

        /// <summary>
        /// Inicializa una nueva instancia de la clase Game llamando al metodo ShowMenu iniciando el programa.
        /// </summary>
        public Game()
        {
            ShowMenu();
        }


        /// <summary>
        /// Método que muestra el menú principal del juego.
        /// </summary>
        public void ShowMenu()
        {
            while (true)
            {
                // Limpia la consola
                AnsiConsole.Clear();

                // Muestra el título del menú principal
                AnsiConsole.Markup("[bold yellow]=== MENÚ PRINCIPAL ===[/]");

                // Pide al usuario que seleccione una opción
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Selecciona una opción:")
                        .AddChoices(new[] { "Empezar a jugar", "Salir del juego" })
                );

                // Maneja la selección del usuario
                if (option == "Empezar a jugar")
                {
                    StartSetup(); // Comienza la configuración del juego
                    break;
                }
                else if (option == "Salir del juego")
                {
                    AnsiConsole.Markup("[bold red]Saliendo del juego...[/]"); // Muestra mensaje de salida
                    return; // Termina el método y sale del juego
                }
            }
        }

        /// <summary>
        /// Configura el juego: pide los nombres de los jugadores y la cantidad de fichas.
        /// </summary>
        public void StartSetup()
        {
            // Pedir el nombre del Jugador 1
            var name1 = AnsiConsole.Ask<string>("Ingresa el nombre del [bold]Jugador 1[/]:");
            Player1 = new Player(name1);

            // Pedir el nombre del Jugador 2
            var name2 = AnsiConsole.Ask<string>("Ingresa el nombre del [bold]Jugador 2[/]:");
            Player2 = new Player(name2);

            // Pedir al usuario la cantidad de fichas hasta que ingrese un número válido
            NumFichas = 0;
            while (NumFichas < 1 || NumFichas > 5)
            {
                NumFichas = AnsiConsole.Ask<int>("Ingresa la cantidad de fichas por jugador (entre 1 y 5):");

                if (NumFichas < 1 || NumFichas > 5)
                {
                    AnsiConsole.Markup("[red]Por favor, ingresa un número entre 1 y 5.[/]\n");
                }
            }

            // Lista de fichas disponibles para la selección
            var availableFichas = new List<Ficha>
    {
        new Ficha("Beedrill", 1, 1, 5, "Agilidad"),
        new Ficha("Sandshrew", 1, 1, 2, "Ventisca"),
        new Ficha("Jigglypuff", 1, 1, 2, "Lullaby"),
        new Ficha("Gengar", 1, 1, 3, "Sombra Trampa"),
        new Ficha("Snivy", 1, 1, 3, "Ignorante"),
        new Ficha("Litwick", 1, 1, 2, "Oversoul"),
        new Ficha("Decidueye", 1, 1, 4, "Sombra Trampa"),
        new Ficha("Litten", 1, 1, 5, "Ignorante"),
        new Ficha("Psyduck", 1, 1, 3, "Lullaby"),
        new Ficha("Greninja", 1, 1, 5, "Sombra Trampa"),
        new Ficha("Charmander", 1, 1, 3, "Oversoul"),
        new Ficha("Bulbasaur", 1, 1, 2, "Lullaby"),
        new Ficha("Squirtle", 1, 1, 4, "Ventisca"),
        new Ficha("Pikachu", 1, 1, 5, "Agilidad"),
        new Ficha("Eevee", 1, 1, 3, "Ignorante"),
        new Ficha("Lucario", 1, 1, 5, "Don Aural"),
        new Ficha("Abra", 1, 1, 1, "Teletransporte")
    };

            // Seleccionar fichas para el Jugador 1
            SelectFichas(Player1, availableFichas, NumFichas);

            // Seleccionar fichas para el Jugador 2
            SelectFichas(Player2, availableFichas, NumFichas);

            // Comenzar el juego
            Start();
        }


        /// <summary>
        /// Permite a un jugador seleccionar fichas del conjunto disponible.
        /// </summary>
        /// <param name="player">El jugador que selecciona las fichas.</param>
        /// <param name="availableFichas">La lista de fichas disponibles.</param>
        /// <param name="numFichas">Cantidad de fichas que se pueden seleccionar.</param>
        private void SelectFichas(Player player, List<Ficha> availableFichas, int numFichas)
        {
            // Muestra un mensaje pidiendo al jugador que seleccione sus fichas
            AnsiConsole.MarkupLine($"[bold green]{player.Name}[/], selecciona tus fichas:");

            int fichasSeleccionadas = 0;

            // Bucle para que el jugador seleccione fichas hasta alcanzar el límite o agotar las opciones disponibles
            while (fichasSeleccionadas < numFichas && availableFichas.Count > 0)
            {
                var options = new List<string>();

                // Añade las opciones de fichas disponibles a la lista de opciones
                foreach (var ficha in availableFichas)
                {
                    options.Add($"{ficha.Name} (Velocidad: {ficha.Speed}, Habilidad: {ficha.HabilidadPredefinida})");
                }

                var fichaPrompt = new SelectionPrompt<string>()
                    .Title("Selecciona una ficha:")
                    .AddChoices(options);

                // Muestra el prompt para que el jugador seleccione una ficha
                string selectedOption = AnsiConsole.Prompt(fichaPrompt);

                // Encuentra la ficha seleccionada 
                Ficha selectedFicha = null;
                foreach (var ficha in availableFichas)
                {
                    if ($"{ficha.Name} (Velocidad: {ficha.Speed}, Habilidad: {ficha.HabilidadPredefinida})" == selectedOption)
                    {
                        selectedFicha = ficha;
                        break;
                    }
                }

                // Añade la ficha seleccionada al jugador y la elimina de la lista de disponibles
                player.AddFicha(selectedFicha);
                availableFichas.Remove(selectedFicha);
                fichasSeleccionadas++;
            }
        }


        /// <summary>
        /// Muestra las fichas del jugador en la consola.
        /// </summary>
        /// <param name="currentPlayer">El jugador cuyas fichas se van a mostrar.</param>
        /// <param name="currentPlayerIndex">Índice del jugador actual.</param>
        
        public void DisplayPlayerFichas(Player currentPlayer, int currentPlayerIndex)
        {
            // Crear una nueva tabla para mostrar las fichas
            Table table = new Table();

            // Configurar el color de la tabla según el jugador actual
            if (currentPlayerIndex == 1)
            {
                table.BorderColor(Color.Green); // Color verde para el Jugador 1
            }
            else if (currentPlayerIndex == 2)
            {
                table.BorderColor(Color.Blue); // Color azul para el Jugador 2
            }

            // Añadir las columnas a la tabla
            table.AddColumn("Índice");
            table.AddColumn("Nombre");
            table.AddColumn("Posición");
            table.AddColumn("Velocidad");
            table.AddColumn("Habilidad");
            table.AddColumn("Cooldown");

            // Colores para los números
            string[] colors = { "red", "green", "yellow", "blue", "magenta", "cyan", "white", "gray" };

            // Añadir las filas a la tabla con los datos de las fichas
            for (int i = 0; i < currentPlayer.Fichas.Count; i++)
            {
                var ficha = currentPlayer.Fichas[i];
                string rowColor = "";

                // Determinar el color de la fila basado en las duraciones de las trampas
                if ((ficha.Duraciones.ContainsKey("Lullaby") && ficha.Duraciones["Lullaby"] > 0) ||
                    (ficha.Duraciones.ContainsKey("Ventisca") && ficha.Duraciones["Ventisca"] > 0) ||
                    (ficha.Duraciones.ContainsKey("Petrification") && ficha.Duraciones["Petrification"] > 0))
                {
                    rowColor = "red"; // Color rojo para las fichas afectadas por una habilidad o trampa de duracion
                }

                // Colores para la posición
                var posicion = $"([bold {colors[ficha.X % colors.Length]}]{ficha.X}[/], [bold {colors[ficha.Y % colors.Length]}]{ficha.Y}[/])";

                // Añadir la fila a la tabla con el color correspondiente
                if (!string.IsNullOrEmpty(rowColor))
                {
                    table.AddRow(new Markup($"[{rowColor}]{i.ToString()}[/]"),
                                 new Markup($"[{rowColor}]{ficha.Name}[/]"),
                                 new Markup($"[{rowColor}]{posicion}[/]"),
                                 new Markup($"[{rowColor}]{ficha.Speed.ToString()}[/]"),
                                 new Markup($"[{rowColor}]{ficha.HabilidadPredefinida}[/]"),
                                 new Markup($"[{rowColor}]{ficha.Cooldowns[ficha.HabilidadPredefinida].ToString()}[/]"));
                }
                else
                {
                    table.AddRow(i.ToString(), ficha.Name, posicion, ficha.Speed.ToString(), ficha.HabilidadPredefinida, ficha.Cooldowns[ficha.HabilidadPredefinida].ToString());
                }
            }

            // Renderizar la tabla en la consola
            AnsiConsole.Render(table);
        }

        /// <summary>
        /// Inicia el juego y gestiona los turnos de los jugadores.
        /// </summary>
        public void Start()
        {
            // Preguntar al usuario por la dificultad
            string dificultad = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selecciona la dificultad:")
                    .AddChoices(new[] { "Tutorial", "Normal", "Pesadilla" }));

            // Configurar el laberinto según la dificultad seleccionada
            switch (dificultad)
            {
                case "Tutorial":
                    Maze = new Maze(8, 8); 
                    Maze.InitializeTutorialMaze(); // Inicializar el laberinto del tutorial
                    break;

                case "Normal":
                    Maze = new Maze(15, 15); 
                    Maze.GenerateMaze(); // Generar el laberinto
                    Maze.PlaceRandomObstaclesAndTraps(9); // Colocar trampas aleatorias
                    break;

                case "Pesadilla":
                    Maze = new Maze(30, 30); 
                    Maze.GenerateMaze(); // Generar el laberinto
                    Maze.PlaceRandomObstaclesAndTraps(30); // Colocar trampas aleatorias
                    break;
            }

            // Inicializar los enfriamientos de las habilidades de las fichas de ambos jugadores
            InitializeCooldowns(Player1.Fichas);
            InitializeCooldowns(Player2.Fichas);

            // Reproducir música dependiendo de la dificultad seleccionada
            string musicFilePath;

            switch (dificultad)
            {
                case "Tutorial":
                    musicFilePath = @"C:\Users\Manuel\source\repos\MazeRunners\Katy Perry - Electric.mp3";
                    break;
                case "Normal":
                    musicFilePath = @"C:\Users\Manuel\source\repos\MazeRunners\Ed Sheeran - Celestial.mp3";
                    break;
                case "Pesadilla":
                    musicFilePath = @"C:\Users\Manuel\source\repos\MazeRunners\Ten Cuidado (Pokémon 25 Version) - J Balvin (320).mp3";
                    break;
                default:
                    musicFilePath = @"C:\Users\Manuel\source\repos\MazeRunners\DefaultMusic.mp3";
                    break;
            }
            musicPlayer.PlayMusic(musicFilePath);

            while (true)
            {
                // Determinar el jugador actual basado en el índice del turno
                Player currentPlayer = currentPlayerIndex == 1 ? Player1 : Player2;
                AnsiConsole.Markup($"[bold blue]Turno de {currentPlayer.Name}.[/]");

                // Mostrar el laberinto y las fichas del jugador actual
                Maze.DisplayMaze(Player1.Fichas, Player2.Fichas, currentPlayerIndex, dificultad);
                DisplayPlayerFichas(currentPlayer, currentPlayerIndex);

                // Pedir al jugador que seleccione la ficha a mover
                int fichaIndex = AnsiConsole.Ask<int>("Selecciona la ficha a mover (índice):");
                currentPlayer.MoveFicha(fichaIndex, Maze.maze, Maze, Player1.Fichas, Player2.Fichas, this, dificultad);

                // Actualizar los enfriamientos de las habilidades del jugador actual
                currentPlayer.ActualizarCooldowns();

                // Cambiar el turno al siguiente jugador
                currentPlayerIndex = currentPlayerIndex == 1 ? 2 : 1;
            }

            // Detener la música al terminar el juego
            musicPlayer.StopMusic();
        }

        /// <summary>
        /// Inicializa los enfriamientos de las habilidades predefinidas de una lista de fichas.
        /// </summary>
        /// <param name="fichas">Lista de fichas.</param>
        private void InitializeCooldowns(List<Ficha> fichas)
        {
            // Iterar sobre cada ficha en la lista
            foreach (var ficha in fichas)
            {
                // Establecer el enfriamiento de la habilidad predefinida a 0
                ficha.Cooldowns[ficha.HabilidadPredefinida] = 0;
            }
        }

        /// <summary>
        /// Muestra un mensaje de victoria al jugador ganador.
        /// </summary>
        /// <param name="nombreJugador">El nombre del jugador que ha ganado.</param>
        public void MostrarMensajeVictoria(string nombreJugador)
        {
            // Crear un panel para mostrar el mensaje de victoria
            var panel = new Panel(new FigletText("¡Muchas Felicidades!")
                .Centered()
                .Color(Color.Green))
            {
                Header = new PanelHeader($"¡Jugador {nombreJugador} ha ganado!"), // Encabezado del panel con el nombre del jugador
                Border = BoxBorder.Double, // Estilo de borde doble
                BorderStyle = new Style(Color.Green), // Color del borde verde
                Padding = new Padding(1, 1) // Espaciado interno del panel
            };

            
            AnsiConsole.Write(panel);

            AnsiConsole.MarkupLine($"[bold underline green]Muchas felicidades {nombreJugador}, acabas de convertirte en el mejor corredor del laberinto![/]");

            AnsiConsole.Markup("[bold red]Presiona cualquier tecla para salir...[/]");

            Console.ReadKey(true);

            // Terminar el juego
            Environment.Exit(0);
        }

        /// <summary>
        /// Cambia el turno entre jugadores, actualizando enfriamientos y verificando si un jugador puede mover sus fichas.
        /// </summary>
        /// <param name="dificultad">La dificultad del laberinto.</param>       
        public void CambiarTurno(string dificultad)
        {
            // Llamar a ActualizarCooldowns para el jugador actual
            Player currentPlayer = currentPlayerIndex == 1 ? Player1 : Player2;
            currentPlayer.ActualizarCooldowns();

            // Cambiar turno entre jugadores
            currentPlayerIndex = currentPlayerIndex == 1 ? 2 : 1;
            currentPlayer = currentPlayerIndex == 1 ? Player1 : Player2;

            // Verificar si todas las fichas están dormidas, congeladas o petrificadas
            bool todasFichasInmoviles = true;
            for (int i = 0; i < currentPlayer.Fichas.Count; i++)
            {
                Ficha ficha = currentPlayer.Fichas[i];
                if ((!ficha.Duraciones.ContainsKey("Lullaby") || ficha.Duraciones["Lullaby"] == 0) &&
                    (!ficha.Duraciones.ContainsKey("Petrification") || ficha.Duraciones["Petrification"] == 0) &&
                    (!ficha.Duraciones.ContainsKey("Ventisca") || ficha.Duraciones["Ventisca"] == 0))
                {
                    todasFichasInmoviles = false;
                    break;
                }
            }

            // Saltar turno del jugador si todas las fichas están dormidas, congeladas o petrificadas
            if (todasFichasInmoviles)
            {
                CambiarTurno(dificultad);
                return;
            }


            // Llamar a DisplayMaze con el índice del jugador actual y la dificultad
            Maze.DisplayMaze(Player1.Fichas, Player2.Fichas, currentPlayerIndex, dificultad);
            DisplayPlayerFichas(currentPlayer, currentPlayerIndex);

            int fichaIndex = AnsiConsole.Ask<int>("Selecciona la ficha a mover (índice):");
            while (fichaIndex < 0 || fichaIndex >= currentPlayer.Fichas.Count ||
                   (currentPlayer.Fichas[fichaIndex].Duraciones.ContainsKey("Lullaby") && currentPlayer.Fichas[fichaIndex].Duraciones["Lullaby"] > 0) ||
                   (currentPlayer.Fichas[fichaIndex].Duraciones.ContainsKey("Petrification") && currentPlayer.Fichas[fichaIndex].Duraciones["Petrification"] > 0) ||
                   (currentPlayer.Fichas[fichaIndex].Duraciones.ContainsKey("Ventisca") && currentPlayer.Fichas[fichaIndex].Duraciones["Ventisca"] > 0))
            {
                if (fichaIndex < 0 || fichaIndex >= currentPlayer.Fichas.Count)
                {
                    Console.WriteLine("Índice inválido. Inténtalo de nuevo.");
                }
                else
                {
                    if (currentPlayer.Fichas[fichaIndex].Duraciones.ContainsKey("Lullaby") && currentPlayer.Fichas[fichaIndex].Duraciones["Lullaby"] > 0)
                    {
                        Console.WriteLine($"La ficha {currentPlayer.Fichas[fichaIndex].Name} está dormida y no puede moverse en este turno. Selecciona otra ficha.");
                    }
                    else if (currentPlayer.Fichas[fichaIndex].Duraciones.ContainsKey("Petrification") && currentPlayer.Fichas[fichaIndex].Duraciones["Petrification"] > 0)
                    {
                        Console.WriteLine($"La ficha {currentPlayer.Fichas[fichaIndex].Name} está petrificada y no puede moverse en este turno. Selecciona otra ficha.");
                    }
                    else if (currentPlayer.Fichas[fichaIndex].Duraciones.ContainsKey("Ventisca") && currentPlayer.Fichas[fichaIndex].Duraciones["Ventisca"] > 0)
                    {
                        Console.WriteLine($"La ficha {currentPlayer.Fichas[fichaIndex].Name} está congelada y no puede moverse en este turno. Selecciona otra ficha.");
                    }
                }
                fichaIndex = AnsiConsole.Ask<int>("Selecciona la ficha a mover (índice):");
            }

            // Mover la ficha seleccionada
            currentPlayer.MoveFicha(fichaIndex, Maze.maze, Maze, Player1.Fichas, Player2.Fichas, this, dificultad);
        }

        /// <summary>
        /// Continúa el turno del jugador actual después de una extensión del turno.
        /// </summary>
        /// <param name="currentPlayer">El jugador que continúa su turno.</param>
        /// <param name="dificultad">La dificultad del laberinto.</param>
        public void ContinueTurn(Player currentPlayer, string dificultad)
        {

            // Pasar el índice del jugador actual a DisplayMaze
            Maze.DisplayMaze(Player1.Fichas, Player2.Fichas, currentPlayerIndex, dificultad);
            DisplayPlayerFichas(currentPlayer, currentPlayerIndex);

            // Pedir al jugador que seleccione la ficha a mover
            int fichaIndex = AnsiConsole.Ask<int>("Selecciona la ficha a mover (índice):");

            // Mover la ficha seleccionada
            currentPlayer.MoveFicha(fichaIndex, Maze.maze, Maze, Player1.Fichas, Player2.Fichas, this, dificultad);

            // Actualizar los enfriamientos de las habilidades del jugador actual
            currentPlayer.ActualizarCooldowns();
        }
    }


}


