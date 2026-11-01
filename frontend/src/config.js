export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Enquanto o back-end em C# não existe, deixe true.
export const USE_MOCK = (import.meta.env.VITE_USE_MOCK ?? 'true') === 'true';

// Quem responde pelos dados pessoais (aparece nas políticas de privacidade e cookies).
// TROQUE pelos dados reais antes de publicar o site.
export const EMPRESA = {
  nome: 'Embarque Excursões',
  emailPrivacidade: 'privacidade@exemplo.com',
};
