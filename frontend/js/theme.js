(function () {
  function aplicarTema(tema) {
    document.documentElement.setAttribute("data-bs-theme", tema);

    if (document.body) {
      document.body.classList.toggle("dark-mode", tema === "dark");
    }
  }

  function actualizarBotonTema(tema) {
    const botonTema = document.getElementById("themeToggle");

    if (!botonTema) return;

    if (tema === "dark") {
      botonTema.classList.add("active");
      botonTema.setAttribute("aria-label", "Cambiar a modo claro");
      botonTema.setAttribute("title", "Cambiar a modo claro");
    } else {
      botonTema.classList.remove("active");
      botonTema.setAttribute("aria-label", "Cambiar a modo oscuro");
      botonTema.setAttribute("title", "Cambiar a modo oscuro");
    }
  }

  const temaGuardado = localStorage.getItem("netcomTheme") || "light";

  document.documentElement.setAttribute("data-bs-theme", temaGuardado);

  document.addEventListener("DOMContentLoaded", function () {
    aplicarTema(temaGuardado);
    actualizarBotonTema(temaGuardado);

    const botonTema = document.getElementById("themeToggle");

    if (!botonTema) return;

    botonTema.addEventListener("click", function () {
      const temaActual = document.documentElement.getAttribute("data-bs-theme") || "light";
      const nuevoTema = temaActual === "dark" ? "light" : "dark";

      aplicarTema(nuevoTema);
      localStorage.setItem("netcomTheme", nuevoTema);
      actualizarBotonTema(nuevoTema);
    });
  });
})();