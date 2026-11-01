import { Routes, Route, Navigate } from 'react-router-dom';
import styled from 'styled-components';
import Navbar from './components/Navbar';
import { Container } from './components/ui.js';
import RotaProtegida from './components/RotaProtegida';
import RodapeSite from './components/RodapeSite';
import AvisoCookies from './components/AvisoCookies';

import Viagens from './pages/Viagens';
import ViagensDisponiveis from './pages/ViagensDisponiveis';
import ViagemDetalhe from './pages/ViagemDetalhe';
import Login from './pages/Login';
import Cadastro from './pages/Cadastro';
import MinhasCompras from './pages/MinhasCompras';
import PainelAdmin from './pages/admin/PainelAdmin';
import ViagemForm from './pages/admin/ViagemForm';
import Passageiros from './pages/admin/Passageiros';
import Privacidade from './pages/Privacidade';
import Cookies from './pages/Cookies';

// rodapé sempre no fim da tela, mesmo em páginas curtas
const Pagina = styled.div`
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  min-height: 100dvh;
`;

const Principal = styled(Container).attrs({ as: 'main' })`
  flex: 1;
  padding-top: 28px;
  padding-bottom: max(64px, env(safe-area-inset-bottom));

  ${({ theme }) => theme.midia.mobile} {
    padding-top: 16px;
    padding-bottom: 40px;
  }
`;

export default function App() {
  return (
    <Pagina>
      <Navbar />
      <Principal>
        <Routes>
          {/* Públicas */}
          <Route path="/" element={<Viagens />} />
          <Route path="/a-venda" element={<ViagensDisponiveis />} />
          <Route path="/viagens/:id" element={<ViagemDetalhe />} />
          <Route path="/login" element={<Login />} />
          <Route path="/cadastro" element={<Cadastro />} />
          <Route path="/privacidade" element={<Privacidade />} />
          <Route path="/cookies" element={<Cookies />} />

          {/* Passageiro */}
          <Route
            path="/minhas-compras"
            element={
              <RotaProtegida tipos={['Passageiro']}>
                <MinhasCompras />
              </RotaProtegida>
            }
          />

          {/* Admin */}
          <Route
            path="/admin"
            element={
              <RotaProtegida tipos={['Admin']}>
                <PainelAdmin />
              </RotaProtegida>
            }
          />
          <Route
            path="/admin/viagens/nova"
            element={
              <RotaProtegida tipos={['Admin']}>
                <ViagemForm />
              </RotaProtegida>
            }
          />
          <Route
            path="/admin/viagens/:id/editar"
            element={
              <RotaProtegida tipos={['Admin']}>
                <ViagemForm />
              </RotaProtegida>
            }
          />
          <Route
            path="/admin/viagens/:id/passageiros"
            element={
              <RotaProtegida tipos={['Admin']}>
                <Passageiros />
              </RotaProtegida>
            }
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Principal>
      <RodapeSite />
      <AvisoCookies />
    </Pagina>
  );
}
