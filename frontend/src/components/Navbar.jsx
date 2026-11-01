import { useEffect, useRef, useState } from 'react';
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom';
import styled, { css } from 'styled-components';
import { useAuth } from '../context/AuthContext';
import { USE_MOCK } from '../config';
import { Botao, Container, Selo } from './ui.js';

const Barra = styled.header`
  background: ${({ theme }) => theme.cores.superficie};
  border-bottom: 1px solid ${({ theme }) => theme.cores.borda};
  position: sticky;
  top: 0;
  z-index: 20;
  padding-top: env(safe-area-inset-top);
`;

const Interno = styled(Container)`
  display: flex;
  align-items: center;
  gap: 24px;
  min-height: ${({ theme }) => theme.alturaNavbar};

  ${({ theme }) => theme.midia.tablet} { gap: 16px; }
  ${({ theme }) => theme.midia.mobile} { min-height: 56px; }
`;

const Marca = styled(Link)`
  font-family: ${({ theme }) => theme.fontes.titulo};
  font-size: 24px;
  font-weight: 700;
  color: ${({ theme }) => theme.cores.texto};
  text-decoration: none;
  flex-shrink: 0;

  span { color: ${({ theme }) => theme.cores.destaque}; }
  ${({ theme }) => theme.midia.mobile} { font-size: 22px; }
`;

const Links = styled.nav`
  display: flex;
  gap: 18px;
  flex: 1;
  min-width: 0;

  ${({ theme }) => theme.midia.mobile} { display: none; }
`;

const Item = styled(NavLink)`
  color: ${({ theme }) => theme.cores.textoSuave};
  text-decoration: none;
  font-weight: 500;
  white-space: nowrap;
  padding: 6px 0;

  &.active {
    color: ${({ theme }) => theme.cores.texto};
    box-shadow: inset 0 -2px 0 ${({ theme }) => theme.cores.destaque};
  }
`;

const Conta = styled.div`
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;

  ${({ theme }) => theme.midia.mobile} { display: none; }
`;

const NomeUsuario = styled.span`
  display: flex;
  flex-direction: column;
  line-height: 1.1;
  font-weight: 600;

  small {
    color: ${({ theme }) => theme.cores.textoSuave};
    font-weight: 400;
    font-size: 12px;
  }
`;

// Selo "modo simulado" some no tablet para sobrar espaço (continua no menu do celular)
const SeloMock = styled(Selo)`
  ${({ theme }) => theme.midia.tablet} { display: none; }
`;

// ---------- Menu do celular ----------
const BotaoMenu = styled.button`
  display: none;
  margin-left: auto;
  width: 44px;
  height: 44px;
  border: 1px solid ${({ theme }) => theme.cores.borda};
  border-radius: 12px;
  background: transparent;
  cursor: pointer;
  place-items: center;

  ${({ theme }) => theme.midia.mobile} { display: grid; }

  /* três tracinhos que viram um X */
  span,
  span::before,
  span::after {
    display: block;
    width: 18px;
    height: 2px;
    border-radius: 2px;
    background: ${({ theme }) => theme.cores.texto};
    transition: transform 0.2s, opacity 0.2s;
  }
  span { position: relative; }
  span::before,
  span::after {
    content: '';
    position: absolute;
    left: 0;
  }
  span::before { top: -6px; }
  span::after { top: 6px; }

  ${({ $aberto }) => $aberto && css`
    span { background: transparent; }
    span::before { transform: translateY(6px) rotate(45deg); }
    span::after { transform: translateY(-6px) rotate(-45deg); }
  `}
`;

const Fundo = styled.div`
  position: fixed;
  inset: 0;
  background: rgba(31, 42, 43, 0.35);
  z-index: 15;
`;

const Gaveta = styled.div`
  position: absolute;
  left: 0;
  right: 0;
  top: 100%;
  background: ${({ theme }) => theme.cores.superficie};
  border-bottom: 1px solid ${({ theme }) => theme.cores.borda};
  box-shadow: 0 12px 24px rgba(31, 42, 43, 0.12);
  padding: 8px 16px 16px;
  padding-bottom: max(16px, env(safe-area-inset-bottom));
  max-height: calc(100dvh - 56px);
  overflow-y: auto;

  nav {
    display: flex;
    flex-direction: column;
  }
`;

const ItemGaveta = styled(NavLink)`
  display: flex;
  align-items: center;
  min-height: 48px;
  padding: 0 4px;
  color: ${({ theme }) => theme.cores.texto};
  text-decoration: none;
  font-weight: 500;
  font-size: 16px;
  border-bottom: 1px solid ${({ theme }) => theme.cores.borda};

  &.active {
    color: ${({ theme }) => theme.cores.primaria};
    font-weight: 600;
  }
`;

const ContaGaveta = styled.div`
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-top: 16px;
`;

const LinhaUsuario = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
`;

export default function Navbar() {
  const { usuario, ehAdmin, ehPassageiro, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [aberto, setAberto] = useState(false);
  const botaoRef = useRef(null);

  // Fecha o menu ao trocar de página
  useEffect(() => setAberto(false), [location.pathname]);

  // Esc fecha; trava a rolagem do fundo enquanto o menu está aberto
  useEffect(() => {
    if (!aberto) return;
    const aoTeclar = (e) => {
      if (e.key === 'Escape') {
        setAberto(false);
        botaoRef.current?.focus();
      }
    };
    document.addEventListener('keydown', aoTeclar);
    const overflowAntes = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', aoTeclar);
      document.body.style.overflow = overflowAntes;
    };
  }, [aberto]);

  // Se a tela crescer (girar o celular, redimensionar), fecha o menu
  useEffect(() => {
    const mq = window.matchMedia('(min-width: 768px)');
    const aoMudar = (e) => e.matches && setAberto(false);
    mq.addEventListener('change', aoMudar);
    return () => mq.removeEventListener('change', aoMudar);
  }, []);

  const sair = () => {
    logout();
    setAberto(false);
    navigate('/');
  };

  const links = (
    <>
      <Item to="/" end>
        Viagens
      </Item>
      <Item to="/a-venda">À venda</Item>
      {ehPassageiro && <Item to="/minhas-compras">Minhas compras</Item>}
      {ehAdmin && <Item to="/admin">Painel admin</Item>}
    </>
  );

  return (
    <>
      {aberto && <Fundo onClick={() => setAberto(false)} aria-hidden="true" />}
      <Barra>
        <Interno>
          <Marca to="/">
            Embarque<span>.</span>
          </Marca>

          <Links aria-label="Principal">{links}</Links>

          <Conta>
            {USE_MOCK && (
              <SeloMock $tipo="aviso" title="Dados simulados no navegador">
                modo simulado
              </SeloMock>
            )}
            {usuario ? (
              <>
                <NomeUsuario>
                  {usuario.nome.split(' ')[0]}
                  <small>{usuario.tipo}</small>
                </NomeUsuario>
                <Botao onClick={sair}>Sair</Botao>
              </>
            ) : (
              <>
                <Botao as={Link} to="/login">
                  Entrar
                </Botao>
                <Botao as={Link} to="/cadastro" $variante="primario">
                  Criar conta
                </Botao>
              </>
            )}
          </Conta>

          <BotaoMenu
            ref={botaoRef}
            $aberto={aberto}
            onClick={() => setAberto((a) => !a)}
            aria-expanded={aberto}
            aria-controls="menu-celular"
            aria-label={aberto ? 'Fechar menu' : 'Abrir menu'}
          >
            <span />
          </BotaoMenu>
        </Interno>

        {aberto && (
          <Gaveta id="menu-celular">
            <nav aria-label="Principal">
              <ItemGaveta to="/" end>
                Viagens
              </ItemGaveta>
              <ItemGaveta to="/a-venda">À venda</ItemGaveta>
              {ehPassageiro && <ItemGaveta to="/minhas-compras">Minhas compras</ItemGaveta>}
              {ehAdmin && <ItemGaveta to="/admin">Painel admin</ItemGaveta>}
            </nav>

            <ContaGaveta>
              {usuario ? (
                <>
                  <LinhaUsuario>
                    <NomeUsuario>
                      {usuario.nome}
                      <small>{usuario.tipo}</small>
                    </NomeUsuario>
                    {USE_MOCK && <Selo $tipo="aviso">modo simulado</Selo>}
                  </LinhaUsuario>
                  <Botao $bloco onClick={sair}>
                    Sair
                  </Botao>
                </>
              ) : (
                <>
                  {USE_MOCK && <Selo $tipo="aviso">modo simulado</Selo>}
                  <Botao as={Link} to="/cadastro" $variante="primario" $bloco>
                    Criar conta
                  </Botao>
                  <Botao as={Link} to="/login" $bloco>
                    Entrar
                  </Botao>
                </>
              )}
            </ContaGaveta>
          </Gaveta>
        )}
      </Barra>
    </>
  );
}
