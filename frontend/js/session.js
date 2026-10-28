function obtenerUsuarioActual() {
  return JSON.parse(localStorage.getItem("usuarioNetcom"));
}

function pintarSesion() {
  const usuario = obtenerUsuarioActual();
  const contenedor = document.getElementById("zonaUsuario");
  const menuCuenta = document.getElementById("menuCuenta");

  if (menuCuenta) {
    if (usuario && (usuario.Rol || usuario.rol) !== "admin") {
      menuCuenta.style.display = "block";
    } else {
      menuCuenta.style.display = "none";
    }
  }

  if (!contenedor) return;

  if (usuario) {
    const nombre = usuario.Nombre || usuario.nombre;
    const rol = usuario.Rol || usuario.rol;

    if (rol === "admin") {
      contenedor.innerHTML = `
        <button class="btn btn-outline-light ms-lg-3" onclick="cerrarSesion()">
          Cerrar sesión
        </button>
      `;
      return;
    }

    contenedor.innerHTML = `
      <div class="dropdown ms-lg-3">
        <button class="btn btn-primary dropdown-toggle" type="button" data-bs-toggle="dropdown">
          ${nombre}
        </button>

        <ul class="dropdown-menu dropdown-menu-end">
          <li><a class="dropdown-item" href="dashboard.html">Mi cuenta</a></li>
          <li><a class="dropdown-item" href="carrito.html">Carrito</a></li>
          <li><button class="dropdown-item" onclick="cerrarSesion()">Cerrar sesión</button></li>
        </ul>
      </div>
    `;
  } else {
    contenedor.innerHTML = `
      <a class="btn btn-primary ms-lg-3" href="login.html">
        Ingresar
      </a>
    `;
  }
}

document.addEventListener("DOMContentLoaded", pintarSesion);