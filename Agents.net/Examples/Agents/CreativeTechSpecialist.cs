using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Models;

namespace Arcana.AgentsNet.Examples.Agents
{
  public class CreativeTechSpecialist : Agent<ContextoCreativeTech, string>
  {
    public CreativeTechSpecialist(IModel model)
        : base(model,
               name: "CreativeTechnologist",
               instructions: @"
🎨 Você é um Creative Technologist premiado com expertise em UX/UI e desenvolvimento!

EXPERTISE CRIATIVA + TÉCNICA:
🎯 User Experience Design
🎨 Visual Design & Branding  
⚡ Rapid Prototyping
🧠 User Research & Testing
⚙️ Technical Implementation
📱 Cross-Platform Development

METODOLOGIA CRIATIVA:
1. DISCOVERY - Research + insights profundos
2. IDEATION - Conceitos inovadores
3. PROTOTIPAGEM - Testes rápidos de ideias
4. DESIGN SYSTEM - Linguagem visual consistente
5. TECHNICAL VALIDATION - Viabilidade técnica
6. USER TESTING - Validação com usuários reais

FERRAMENTAS ESPECIALIZADAS:
🎨 Figma, Sketch, Adobe Creative Suite
⚡ InVision, Principle, Framer
🧠 Hotjar, Mixpanel, UserTesting
⚙️ React, React Native, TypeScript

Seja criativo, técnico e centrado no usuário!")
    {
    }

    [FunctionCall("Pesquisa de usuário especializada")]
    [FunctionCallParameter("targetAudience", "Público-alvo para pesquisa")]
    [FunctionCallParameter("researchMethods", "Métodos de pesquisa (interviews, surveys, analytics)")]
    private string UserResearchAvancada(string targetAudience, string researchMethods)
    {
      return $@"
👥 USER RESEARCH: {targetAudience.ToUpper()}
═══════════════════════════════════════

🎯 PERFIL DO USUÁRIO:
• Idade: {Context.BriefProjeto.Publico}
• Comportamento: Digital natives, mobile-first
• Pain Points: Complexidade financeira, falta confiança
• Motivações: Independência financeira, gamification
• Canais: Instagram, TikTok, YouTube, Discord

📊 INSIGHTS QUANTITATIVOS:
• 78% nunca investiram antes
• 65% usam apps bancários diariamente  
• 45% interessados em educação financeira
• 82% preferem interfaces gamificadas
• 71% valorizam transparência total

💡 OPPORTUNITIES IDENTIFICADAS:
• Educação financeira como onboarding
• Micro-investimentos (R$ 10-50)
• Social features para compartilhamento
• Gamification com recompensas reais
• Simuladores de investimento

🚀 DESIGN PRINCIPLES:
• Simplicidade sem dumbing down
• Transparência absoluta
• Feedback imediato
• Progressão visível
• Community-driven learning";
    }

    [FunctionCall("Prototipagem interativa avançada")]
    [FunctionCallParameter("features", "Features principais para prototipar")]
    [FunctionCallParameter("platform", "Plataforma alvo (iOS, Android, Web)")]
    private string PrototipagemInterativa(string features, string platform)
    {
      return $@"
⚡ PROTÓTIPO INTERATIVO: {platform.ToUpper()}
═══════════════════════════════════════

🎮 CORE FEATURES PROTOTIPADAS:
• Onboarding gamificado (5 steps + quiz)
• Dashboard principal com portfolio visual
• Micro-investimento flow (1-tap investing)
• Educational modules com progresso
• Social sharing de conquistas

🎨 INTERACTION DESIGN:
• Swipe gestures para navegação
• Pull-to-refresh em todas as listas
• Haptic feedback em ações críticas
• Animations micro para delight
• Progressive disclosure de complexidade

📱 RESPONSIVE CONSIDERATIONS:
• Mobile-first design approach
• Thumb-friendly touch targets (44px+)
• Portrait orientation primária
• Dark mode support completo
• Acessibilidade AA compliance

🔄 USER FLOWS MAPEADOS:
1. Signup → Profile → Education → First Investment
2. Dashboard → Explore → Research → Invest
3. Portfolio → Performance → Share → Celebrate
4. Learning → Quiz → Badge → Unlock Feature

⚙️ TECHNICAL SPECS:
• React Native + TypeScript
• Redux para state management
• Async storage para persistência
• Biometric authentication
• Real-time data sync";
    }

    [FunctionCall("Sistema de design escalável")]
    [FunctionCallParameter("brandValues", "Valores da marca para traduzir visualmente")]
    [FunctionCallParameter("platforms", "Plataformas que o design system deve cobrir")]
    private string DesignSystemEscalavel(string brandValues, string platforms)
    {
      return $@"
🎨 DESIGN SYSTEM: {Context.BriefProjeto.Cliente.ToUpper()}
═══════════════════════════════════════

🎯 BRAND VALUES → VISUAL LANGUAGE:
• Trust → Clean layouts, consistent spacing
• Modern → Bold typography, vibrant accents
• Gamified → Progress bars, achievement badges
• Accessible → High contrast, large touch targets

🎨 COLOR SYSTEM:
• Primary: #0066FF (Trust Blue)
• Secondary: #00D4AA (Success Green)  
• Accent: #FF6B35 (Energy Orange)
• Neutral: #F8F9FA → #212529 (8 shades)
• Semantic: Success, Warning, Error, Info

📝 TYPOGRAPHY SCALE:
• Display: Inter Black (32px, 28px, 24px)
• Heading: Inter Bold (20px, 18px, 16px)
• Body: Inter Regular (14px, 12px)
• Caption: Inter Medium (10px)

🧩 COMPONENT LIBRARY:
• Buttons (Primary, Secondary, Ghost, Icon)
• Cards (Portfolio, Education, Achievement)
• Forms (Input, Select, Toggle, Slider)
• Navigation (Tab Bar, Side Menu, Breadcrumb)
• Data Display (Charts, Graphs, Stats)
• Feedback (Toast, Modal, Loader)

⚙️ DOCUMENTAÇÃO TÉCNICA:
• Storybook com live components
• Código implementado em React
• Props API documentação completa
• Accessibility guidelines
• Integration examples";
    }
  }
}
