import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/**
 * <RotaProtegida tipos={['Admin']}> ... </RotaProtegida>
 * Isso só esconde a tela. Quem realmente protege é o [Authorize(Roles = ...)] no back.
 */
export default function RotaProtegida({ tipos, children }) {
  const { usuario } = useAuth();
  const location = useLocation();

  if (!usuario) return <Navigate to="/login" replace state={{ de: location.pathname }} />;
  if (tipos && !tipos.includes(usuario.tipo)) return <Navigate to="/" replace />;
  return children;
}
