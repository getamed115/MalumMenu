from __future__ import annotations

import secrets
import string
import subprocess
import sys
from pathlib import Path
from typing import NoReturn, Sequence

from rich import box
from rich.console import Console
from rich.markup import escape
from rich.panel import Panel
from rich.table import Table


CODE_LENGTH = 4
ALLOWED_CHARACTERS = string.ascii_lowercase + string.digits
DEFAULT_REMOTE = "origin"

console = Console()
error_console = Console(stderr=True)


def run_git(
    *arguments: str,
    capture_output: bool = False,
    timeout: int | None = None,
) -> subprocess.CompletedProcess:
    """
    Execute a Git command.

    If capture_output is True, stdout and stderr are captured.
    Otherwise, Git output is displayed directly in the terminal.
    """
    command = ["git", *arguments]

    return subprocess.run(
        command,
        capture_output=capture_output,
        text=True,
        encoding="utf-8",
        errors="replace",
        timeout=timeout,
        check=False,
    )


def fail(message: str, exit_code: int = 1) -> NoReturn:
    """Display an error and terminate the program."""
    error_console.print(
        Panel(
            escape(message),
            title="[bold red]Errore[/bold red]",
            border_style="red",
            box=box.ROUNDED,
        )
    )

    raise SystemExit(exit_code)


def warning(message: str) -> None:
    """Display a warning without terminating the program."""
    error_console.print(
        Panel(
            escape(message),
            title="[bold yellow]Avviso[/bold yellow]",
            border_style="yellow",
            box=box.ROUNDED,
        )
    )


def format_git_error(
    result: subprocess.CompletedProcess[str],
    fallback_message: str,
) -> str:
    """
    Return stderr or stdout produced by Git.

    If Git produced no useful output, return the fallback message.
    """
    stderr = result.stderr.strip() if result.stderr else ""
    stdout = result.stdout.strip() if result.stdout else ""

    details = stderr or stdout

    if not details:
        return fallback_message

    return f"{fallback_message}\n\nDettagli Git:\n{details}"


def check_git_available() -> str:
    """Check whether Git is installed and return its version."""
    try:
        result = run_git("--version", capture_output=True, timeout=10)
    except FileNotFoundError:
        fail("Git non è installato oppure non è disponibile nel PATH.")
    except subprocess.TimeoutExpired:
        fail("Il controllo della versione di Git ha superato il tempo limite.")

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                "Git non è installato oppure non è disponibile nel PATH.",
            )
        )

    return result.stdout.strip()


def get_repository_root() -> Path:
    """Check the repository and return its root directory."""
    result = run_git(
        "rev-parse",
        "--show-toplevel",
        capture_output=True,
        timeout=10,
    )

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                "La directory corrente non appartiene a un repository Git.",
            )
        )

    repository_root = result.stdout.strip()

    if not repository_root:
        fail("Git non ha restituito la directory principale del repository.")

    return Path(repository_root)


def stage_changes() -> None:
    """Stage all tracked, untracked and deleted files."""
    with console.status(
        "[bold cyan]Preparazione delle modifiche...[/bold cyan]",
        spinner="dots",
    ):
        result = run_git("add", "-A", capture_output=True)

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                "Non è stato possibile preparare le modifiche.",
            )
        )

    console.print("[green]✓[/green] Modifiche preparate con successo.")


def staged_changes_exist() -> bool:
    """Return True when there are staged changes."""
    result = run_git(
        "diff",
        "--cached",
        "--quiet",
        capture_output=True,
    )

    if result.returncode == 0:
        return False

    if result.returncode == 1:
        return True

    fail(
        format_git_error(
            result,
            "Errore durante il controllo delle modifiche preparate.",
        )
    )


def fetch_remote_references() -> bool:
    """
    Update remote references.

    Return False when fetching fails, allowing the script to continue
    with the locally available history.
    """
    with console.status(
        "[bold cyan]Aggiornamento dei riferimenti remoti...[/bold cyan]",
        spinner="dots",
    ):
        try:
            result = run_git(
                "fetch",
                "--all",
                "--prune",
                capture_output=True,
                timeout=120,
            )
        except subprocess.TimeoutExpired:
            warning(
                "Il fetch ha superato il limite di 120 secondi. "
                "Verrà utilizzata la cronologia disponibile localmente."
            )
            return False

    if result.returncode != 0:
        warning(
            format_git_error(
                result,
                "Il fetch non è riuscito. Verrà utilizzata soltanto "
                "la cronologia Git disponibile localmente.",
            )
        )
        return False

    console.print("[green]✓[/green] Riferimenti remoti aggiornati.")
    return True


def get_used_commit_messages() -> set:
    """Return the subjects of all locally known commits."""
    result = run_git(
        "log",
        "--all",
        "--format=%s",
        capture_output=True,
    )

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                "Non è stato possibile leggere la cronologia dei commit.",
            )
        )

    return {
        message.strip()
        for message in result.stdout.splitlines()
        if message.strip()
    }


def generate_unique_commit_message(
    used_messages: set[str],
) -> tuple[str, int]:
    """
    Generate a random code not already used as a complete commit subject.

    Return the generated code and the number of attempts required.
    """
    maximum_combinations = len(ALLOWED_CHARACTERS) ** CODE_LENGTH

    used_codes = {
        message
        for message in used_messages
        if (
            len(message) == CODE_LENGTH
            and all(
                character in ALLOWED_CHARACTERS
                for character in message
            )
        )
    }

    if len(used_codes) >= maximum_combinations:
        fail(
            f"Non sono disponibili altri codici di {CODE_LENGTH} caratteri."
        )

    attempts = 0

    while True:
        attempts += 1

        commit_message = "".join(
            secrets.choice(ALLOWED_CHARACTERS)
            for _ in range(CODE_LENGTH)
        )

        if commit_message not in used_codes:
            return commit_message, attempts


def create_commit(commit_message: str) -> None:
    """Create the Git commit."""
    safe_message = escape(commit_message)

    console.print(
        f"\n[bold cyan]Creazione del commit "
        f"[white]'{safe_message}'[/white]...[/bold cyan]"
    )

    result = run_git(
        "commit",
        "-m",
        commit_message,
        capture_output=True,
    )

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                "La creazione del commit non è riuscita. "
                "Il push non verrà eseguito.",
            )
        )

    console.print(result.stdout.strip())
    console.print("[green]✓[/green] Commit creato localmente.")


def get_current_branch() -> str:
    """Return the current branch name."""
    result = run_git(
        "branch",
        "--show-current",
        capture_output=True,
    )

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                "Il commit è stato creato, ma non è stato possibile "
                "determinare il branch corrente.",
            )
        )

    current_branch = result.stdout.strip()

    if not current_branch:
        fail(
            "Il commit è stato creato localmente, ma il repository si trova "
            "in stato detached HEAD. Il push non è stato eseguito."
        )

    return current_branch


def get_upstream_branch() -> str | None:
    """Return the upstream branch, or None if it is not configured."""
    result = run_git(
        "rev-parse",
        "--abbrev-ref",
        "--symbolic-full-name",
        "@{upstream}",
        capture_output=True,
    )

    if result.returncode != 0:
        return None

    upstream = result.stdout.strip()
    return upstream or None


def remote_exists(remote_name: str) -> bool:
    """Return True if the given Git remote exists."""
    result = run_git(
        "remote",
        "get-url",
        remote_name,
        capture_output=True,
    )

    return result.returncode == 0


def push_commit(
    current_branch: str,
    commit_message: str,
) -> str:
    """
    Push the current branch.

    Return the upstream destination used for the push.
    """
    safe_branch = escape(current_branch)

    console.print(
        f"\n[bold cyan]Push del branch "
        f"[white]'{safe_branch}'[/white]...[/bold cyan]"
    )

    upstream = get_upstream_branch()

    try:
        if upstream is not None:
            result = run_git(
                "push",
                capture_output=True,
                timeout=120,
            )
            destination = upstream
        else:
            if not remote_exists(DEFAULT_REMOTE):
                fail(
                    f"Il commit '{commit_message}' è stato creato localmente, "
                    f"ma il remote '{DEFAULT_REMOTE}' non esiste. "
                    "Il push non è stato eseguito."
                )

            destination = f"{DEFAULT_REMOTE}/{current_branch}"

            console.print(
                "[yellow]![/yellow] Nessun upstream configurato. "
                f"Verrà configurato [bold]{escape(destination)}[/bold]."
            )

            result = run_git(
                "push",
                "--set-upstream",
                DEFAULT_REMOTE,
                current_branch,
                capture_output=True,
                timeout=120,
            )

    except subprocess.TimeoutExpired:
        fail(
            f"Il push ha superato il limite di 120 secondi. "
            f"Il commit '{commit_message}' è comunque disponibile localmente."
        )

    if result.returncode != 0:
        fail(
            format_git_error(
                result,
                f"Il push non è riuscito, ma il commit '{commit_message}' "
                "è stato creato correttamente in locale.",
            )
        )

    if result.stdout.strip():
        console.print(result.stdout.strip())

    if result.stderr.strip():
        console.print(result.stderr.strip(), style="dim")

    console.print("[green]✓[/green] Push completato.")
    return destination


def show_header(repository_root: Path, git_version: str) -> None:
    """Display initial repository information."""
    table = Table(
        show_header=False,
        box=None,
        padding=(0, 1),
    )

    table.add_column(style="bold cyan")
    table.add_column()

    table.add_row("Repository", str(repository_root))
    table.add_row("Git", git_version)
    table.add_row("Codice", f"{CODE_LENGTH} caratteri")
    table.add_row(
        "Combinazioni",
        f"{len(ALLOWED_CHARACTERS) ** CODE_LENGTH:,}".replace(",", "."),
    )

    console.print(
        Panel(
            table,
            title="[bold blue]Commit automatico[/bold blue]",
            border_style="blue",
            box=box.ROUNDED,
        )
    )


def show_success_summary(
    repository_root: Path,
    branch: str,
    upstream: str,
    commit_message: str,
    known_messages: int,
    generation_attempts: int,
    fetch_succeeded: bool,
) -> None:
    """Display the final operation summary."""
    table = Table(
        box=box.SIMPLE,
        show_header=False,
        padding=(0, 2),
    )

    table.add_column(style="bold")
    table.add_column()

    table.add_row("Repository", str(repository_root))
    table.add_row("Branch", escape(branch))
    table.add_row("Destinazione", escape(upstream))
    table.add_row(
        "Commit",
        f"[bold green]{escape(commit_message)}[/bold green]",
    )
    table.add_row("Messaggi conosciuti", str(known_messages))
    table.add_row("Tentativi generazione", str(generation_attempts))
    table.add_row(
        "Fetch",
        (
            "[green]Completato[/green]"
            if fetch_succeeded
            else "[yellow]Non completato[/yellow]"
        ),
    )

    console.print()
    console.print(
        Panel(
            table,
            title="[bold green]Operazione completata[/bold green]",
            border_style="green",
            box=box.ROUNDED,
        )
    )


def main(arguments: Sequence[str] | None = None) -> int:
    """
    Stage changes, generate a unique commit message, commit and push.

    The arguments parameter is reserved for future command-line options.
    """
    del arguments

    git_version = check_git_available()
    repository_root = get_repository_root()

    show_header(repository_root, git_version)

    stage_changes()

    if not staged_changes_exist():
        console.print(
            Panel(
                "Non ci sono modifiche da includere in un commit.",
                title="[bold yellow]Nessuna modifica[/bold yellow]",
                border_style="yellow",
                box=box.ROUNDED,
            )
        )
        return 0

    fetch_succeeded = fetch_remote_references()
    used_messages = get_used_commit_messages()

    commit_message, generation_attempts = (
        generate_unique_commit_message(used_messages)
    )

    console.print(
        f"[green]✓[/green] Codice univoco generato: "
        f"[bold white on dark_green] {escape(commit_message)} [/bold white on dark_green]"
    )

    create_commit(commit_message)

    current_branch = get_current_branch()
    upstream = push_commit(current_branch, commit_message)

    show_success_summary(
        repository_root=repository_root,
        branch=current_branch,
        upstream=upstream,
        commit_message=commit_message,
        known_messages=len(used_messages),
        generation_attempts=generation_attempts,
        fetch_succeeded=fetch_succeeded,
    )

    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main(sys.argv[1:]))

    except KeyboardInterrupt:
        error_console.print(
            "\n[yellow]Operazione annullata dall'utente.[/yellow]"
        )
        raise SystemExit(130)

    except subprocess.SubprocessError as error:
        fail(f"Errore durante l'esecuzione di Git: {error}")

    except OSError as error:
        fail(f"Errore del sistema operativo: {error}")
