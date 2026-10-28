const API_PRODUCTO = "https://localhost:44301/api/producto";

let products = [];

function formatoPrecio(valor) {
  return "$" + Number(valor).toLocaleString("es-CO");
}

async function cargarProductosDesdeAPI() {
  try {
    const respuesta = await fetch(API_PRODUCTO);

    if (!respuesta.ok) {
      throw new Error("No se pudieron cargar los productos.");
    }

    products = await respuesta.json();
    return products;
  } catch (error) {
    console.error(error);
    alert("Error al cargar productos desde la base de datos.");
    return [];
  }
}

function obtenerImagenProducto(producto) {
  const imagen =
    producto.Imagen ||
    producto.imagen ||
    producto.image ||
    producto.NombreProducto ||
    producto.nombreProducto ||
    producto.name;

  if (!imagen) {
    return "https://via.placeholder.com/400x250?text=Sin+Imagen";
  }

  if (
    imagen.startsWith("http") ||
    imagen.startsWith("img/") ||
    imagen.startsWith("./img/")
  ) {
    return imagen;
  }

  return `img/productos/${imagen}.jpg`;
}
function normalizarCategoria(nombreCategoria) {
  const nombre = (nombreCategoria || "").toLowerCase();

  if (nombre.includes("port")) return "portatiles";
  if (nombre.includes("tel")) return "telefonos";
  if (nombre.includes("consol")) return "consolas";

  return "otros";
}

function agregarCarrito(idProducto) {
  const usuario = JSON.parse(localStorage.getItem("usuarioNetcom"));

  if (!usuario) {
    alert("Debes iniciar sesión para comprar.");
    window.location.href = "login.html";
    return;
  }

  const rol = usuario.Rol || usuario.rol;

  if (rol === "admin") {
    alert("El administrador no puede comprar ni agregar productos al carrito.");
    return;
  }

  const producto = products.find(p =>
    (p.IdProducto || p.idProducto || p.id) === idProducto
  );

  if (!producto) {
    alert("Este producto no existe en la base de datos.");
    return;
  }

  const id = producto.IdProducto || producto.idProducto || producto.id;
  const nombre = producto.NombreProducto || producto.nombreProducto || producto.name;
  const categoria = producto.NombreCategoria || producto.nombreCategoria || producto.category;
  const precio = producto.Precio || producto.precio || producto.price;
  const stock = producto.Stock ?? producto.stock;

  if (stock <= 0) {
    alert("Este producto está agotado.");
    return;
  }

  let carrito = JSON.parse(localStorage.getItem("carrito")) || [];

  const existente = carrito.find(p => p.idProducto === id);

  if (existente) {
    if (existente.cantidad + 1 > stock) {
      alert("No hay más stock disponible.");
      return;
    }

    existente.cantidad += 1;
  } else {
    carrito.push({
      idProducto: id,
      nombre: nombre,
      categoria: categoria,
      precio: Number(precio),
      stock: Number(stock),
      cantidad: 1
    });
  }

  localStorage.setItem("carrito", JSON.stringify(carrito));
  alert("Producto agregado al carrito.");
}