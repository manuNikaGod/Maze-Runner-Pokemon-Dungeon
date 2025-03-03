using System;
using System.Collections.Generic;
using System.Linq;
using MazeRunners;
using Spectre.Console;

/// <summary>
/// Representa un jugador en el juego.
/// </summary>
public class Player
{
    /// <summary>
    /// Obtiene o establece el nombre.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indica si el jugador tiene un turno extra.
    /// </summary>
    public bool TurnoExtra { get; set; }

    /// <summary>
    /// Obtiene o establece la lista de fichas del jugador.
    /// </summary>
    public List<Ficha> Fichas { get; set; }

    /// <summary>
    /// Obtiene o establece el índice del jugador actual.
    /// </summary>
    public int CurrentPlayerIndex { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase Player.
    /// </summary>
    /// <param name="name">Nombre del jugador.</param>
    public Player(string name)
    {
        // Asigna el nombre del jugador 
        Name = name;

        // Inicializa la lista de fichas del jugador como una nueva lista vacía
        Fichas = new List<Ficha>();
    }

    /// <summary>
    /// Añade una ficha a la lista de fichas del jugador.
    /// </summary>
    /// <param name="ficha">La ficha que se va a añadir.</param>
    public void AddFicha(Ficha ficha)
    {
        // Verifica si el número de fichas actuales es menor que 5
        if (Fichas.Count < 5)
        {
            // Añade la ficha a la lista de fichas del jugador
            Fichas.Add(ficha);
        }
    }

    /// <summary>
    /// Mueve una ficha del jugador usando las letras A,S,D,W del teclado.
    /// </summary>
    /// <param name="fichaIndex">Índice de la ficha en la lista de fichas del jugador.</param>
    /// <param name="maze">El laberinto en el que se mueve la ficha.</param>
    /// <param name="mazeInstance">Instancia del laberinto para actualizar la visualización.</param>
    /// <param name="player1Fichas">Lista de fichas del Jugador 1.</param>
    /// <param name="player2Fichas">Lista de fichas del Jugador 2.</param>
    /// <param name="game">Instancia del juego para controlar el flujo del turno y la victoria.</param>
    /// <param name="dificultad">La dificultad del laberinto.</param>
    public void MoveFicha(int fichaIndex, int[,] maze, Maze mazeInstance, List<Ficha> player1Fichas, List<Ficha> player2Fichas, Game game, string dificultad)
    {
        // Verificar si el índice de la ficha es válido
        if (fichaIndex < 0 || fichaIndex >= Fichas.Count)
        {
            Console.WriteLine("Índice de ficha no válido. Inténtalo nuevamente.");
            return; // Salir del método si el índice no es válido
        }

        // Obtener la ficha a mover
        Ficha ficha = Fichas[fichaIndex];


        // Verificar si la ficha está afectada por un efecto y no puede moverse
        if ((ficha.Duraciones.ContainsKey("Lullaby") && ficha.Duraciones["Lullaby"] > 0) ||
            (ficha.Duraciones.ContainsKey("Petrification") && ficha.Duraciones["Petrification"] > 0) ||
            (ficha.Duraciones.ContainsKey("Ventisca") && ficha.Duraciones["Ventisca"] > 0))
        {
            // Verificar si hay otras fichas disponibles sin efectos
            bool hayFichasSinEfectos = false;
            for (int i = 0; i < Fichas.Count; i++)
            {
                if (!Fichas[i].Duraciones.ContainsKey("Lullaby") && !Fichas[i].Duraciones.ContainsKey("Petrification") &&
                    !Fichas[i].Duraciones.ContainsKey("Ventisca"))
                {
                    hayFichasSinEfectos = true;
                    break;
                }
            }
            if (hayFichasSinEfectos)
            {
                return; // Salir del método si hay fichas disponibles sin efectos
            }

            // Cambiar el turno si no hay fichas sin efectos
            game.CambiarTurno(dificultad);
            return;
        }

        // Inicializar los movimientos restantes de la ficha
        int movimientosRestantes = ficha.Speed;
        Console.WriteLine($"Movimientos disponibles para {ficha.Name}: {movimientosRestantes}");

        // Verificar si la habilidad predefinida está disponible y preguntar si se desea activar
        if (!ficha.Cooldowns.ContainsKey(ficha.HabilidadPredefinida) || ficha.Cooldowns[ficha.HabilidadPredefinida] == 0)
        {
            bool activarHabilidad = AnsiConsole.Confirm($"¿Deseas activar la habilidad {ficha.HabilidadPredefinida} de la ficha {ficha.Name}?");
            if (activarHabilidad)
            {
                List<Ficha> propiasFichas = this.Fichas;
                List<Ficha> fichasEnemigas = game.currentPlayerIndex == 1 ? player2Fichas : player1Fichas;
                ficha.UseHabilidad(ficha.HabilidadPredefinida, fichasEnemigas, mazeInstance, this, propiasFichas, ref movimientosRestantes, dificultad);
            }
        }
        else
        {
            Console.WriteLine($"La habilidad {ficha.HabilidadPredefinida} de {ficha.Name} está en enfriamiento.");
        }

        // Inicializar el estado de teletransportación
        bool teletransportado = false;

        // Procesar el movimiento de la ficha usando las teclas del teclado
        while (movimientosRestantes > 0)
        {
            ConsoleKey key = Console.ReadKey(true).Key; // Leer la tecla presionada por el usuario
            int deltaX = 0, deltaY = 0;

            // Determinar la dirección del movimiento según la tecla presionada
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    deltaX = -1;
                    break;
                case ConsoleKey.RightArrow:
                    deltaX = 1;
                    break;
                case ConsoleKey.UpArrow:
                    deltaY = -1;
                    break;
                case ConsoleKey.DownArrow:
                    deltaY = 1;
                    break;
                default:
                    Console.WriteLine("Tecla inválida. Inténtalo nuevamente.");
                    continue; // Continuar el bucle si la tecla es inválida
            }

            // Verificar la nueva posición antes de mover la ficha
            int newX = ficha.X + deltaX;
            int newY = ficha.Y + deltaY;

            // Comprobar si la nueva posición es válida
            bool posicionValida = newX >= 0 && newY >= 0 && newX < maze.GetLength(1) && newY < maze.GetLength(0) &&
                                  (maze[newY, newX] == Maze.PATH || maze[newY, newX] == Maze.TRAP_SLOW || maze[newY, newX] == Maze.TRAP_CONFUSION || maze[newY, newX] == Maze.TRAP_TELEPORT);
            if(posicionValida)
            {
                // Verificar si la ficha ha llegado a la meta ANTES de moverla
                if (newX == maze.GetLength(1) - 2 && newY == maze.GetLength(0) - 2)
                {
                    Console.WriteLine("¡Ficha ha llegado a la meta!");
                    Fichas.RemoveAt(fichaIndex);

                    if (Fichas.Count == 0)
                    {
                        game.MostrarMensajeVictoria(Name);
                        return;
                    }
                    break;
                }

                ficha.Move(deltaX, deltaY, maze, ref teletransportado, ref movimientosRestantes);

                // Verificar si la ficha ha sido teletransportada
                if (teletransportado)
                {
                    teletransportado = false;
                    // Actualizar la visualización del laberinto
                    AnsiConsole.Clear();
                    mazeInstance.DisplayMaze(player1Fichas, player2Fichas, game.currentPlayerIndex, dificultad);
                    continue; // Continuar el bucle para el próximo movimiento sin disminuir más movimientosRestantes
                }

                movimientosRestantes--; // Reducir movimientos restantes por movimiento regular
                Console.WriteLine($"Movimientos restantes: {movimientosRestantes}");

                // Actualizar la visualización del laberinto después de cada movimiento
                AnsiConsole.Clear();
                mazeInstance.DisplayMaze(player1Fichas, player2Fichas, game.currentPlayerIndex, dificultad);
                Console.WriteLine($"Movimientos restantes para {ficha.Name}: {movimientosRestantes}");
            }
            else
            {
                Console.WriteLine("Posición inválida. Intenta nuevamente.");
                Console.WriteLine($"Intentando mover a ({newX}, {newY})");
                continue;
            }
        }

        // Actualizar los enfriamientos de las habilidades del jugador actual
        ActualizarCooldowns();

        // Verificar si el jugador tiene un turno extra
        if (TurnoExtra)
        {
            TurnoExtra = false;
            game.ContinueTurn(this, dificultad); // Continuar el turno si hay un turno extra
            return;
        }

        // Cambia el turno al siguiente jugador
        game.CambiarTurno(dificultad);
    }

    /// <summary>
    /// Actualiza los cooldowns de habilidades y las duraciones de efectos temporales para cada ficha del jugador.
    /// </summary>
    public void ActualizarCooldowns()
    {
        for (int j = 0; j < Fichas.Count; j++)
        {
            Ficha ficha = Fichas[j];

            // Reducir el cooldown de las habilidades
            List<string> habilidades = new List<string>(ficha.Cooldowns.Keys);
            foreach (var habilidad in habilidades)
            {
                if (ficha.Cooldowns.ContainsKey(habilidad) && ficha.Cooldowns[habilidad] > 0)
                {
                    ficha.Cooldowns[habilidad]--;
                    Console.WriteLine($"Cooldown de la habilidad {habilidad} de {ficha.Name} reducido a: {ficha.Cooldowns[habilidad]}");
                }
            }

            // Reducir las duraciones de los efectos temporales
            List<string> duraciones = new List<string>(ficha.Duraciones.Keys);
            foreach (var duracion in duraciones)
            {
                if (ficha.Duraciones.ContainsKey(duracion) && ficha.Duraciones[duracion] > 0)
                {
                    ficha.Duraciones[duracion]--;
                }

                // Verificación y restauración de la velocidad al terminar el efecto Oversoul
                if (duracion == "Oversoul" && ficha.Duraciones[duracion] == 0)
                {
                    ficha.Speed = ficha.InitialSpeed;
                    Console.WriteLine($"Velocidad de {ficha.Name} restaurada a: {ficha.Speed}");
                    ficha.Duraciones.Remove(duracion);
                }
            }
        }
    }

}
