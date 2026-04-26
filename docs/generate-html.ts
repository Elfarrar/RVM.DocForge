/**
 * RVM.DocForge — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone.
 *
 * Uso:
 *   cd docs && npx tsx generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  {
    id: 'home',
    title: '1. Dashboard Principal',
    description:
      'Painel central do RVM.DocForge. Exibe estatisticas globais da documentacao gerada: ' +
      'projetos cadastrados, documentos produzidos e atividade recente.',
    screenshot: '01-home',
    features: [
      'Contagem de projetos e documentos gerados',
      'Atividade recente de geracao de documentacao',
      'Atalhos rapidos para projetos e documentos',
      'Status do motor de analise Roslyn',
    ],
  },
  {
    id: 'projects',
    title: '2. Projetos',
    description:
      'Gerenciamento de projetos .NET monitorados pelo DocForge. ' +
      'Cada projeto representa um repositorio ou solucao que sera documentada automaticamente.',
    screenshot: '02-projects',
    features: [
      'Listagem de projetos cadastrados',
      'Adicionar novo projeto (URL Git ou caminho local)',
      'Status de documentacao por projeto (atualizado/desatualizado)',
      'Acesso rapido para gerar ou visualizar documentacao',
      'Remocao de projetos',
    ],
    tips: [
      'Vincule o projeto ao repositorio Git para rastrear mudancas automaticamente.',
      'O DocForge monitora commits e regenera a documentacao quando detecta alteracoes no codigo.',
    ],
  },
  {
    id: 'documents',
    title: '3. Documentos',
    description:
      'Biblioteca de documentos gerados automaticamente via analise Roslyn. ' +
      'Suporta 7 formatos de saida: Markdown, HTML, PDF, Word, JSON, XML e OpenAPI.',
    screenshot: '03-documents',
    features: [
      'Listagem de todos os documentos gerados',
      'Filtro por projeto, formato e data de geracao',
      'Preview inline do documento',
      'Download em multiplos formatos',
      'Historico de versoes por documento',
    ],
    tips: [
      'Use o formato OpenAPI para gerar especificacoes que podem ser importadas no Postman ou Swagger UI.',
      'O formato HTML inclui navegacao lateral e busca full-text.',
    ],
  },
  {
    id: 'api-docs',
    title: '4. Geracao de Documentacao',
    description:
      'Interface para acionar manualmente a geracao de documentacao de um projeto. ' +
      'Permite selecionar o projeto, formato de saida e opcoes de configuracao.',
    screenshot: '04-api-docs',
    features: [
      'Selecao de projeto e formato de saida',
      'Opcoes: incluir exemplos, metodos privados, XML docs',
      'Progresso de geracao em tempo real',
      'Preview do documento gerado',
      'Download imediato apos conclusao',
    ],
  },
  {
    id: 'analysis',
    title: '5. Analise de Codigo',
    description:
      'Relatorio de cobertura de documentacao: quais tipos, metodos e propriedades ' +
      'possuem XML docs e quais estao sem documentacao.',
    screenshot: '05-analysis',
    features: [
      'Percentual de cobertura de documentacao por assembly',
      'Lista de membros sem XML doc comments',
      'Sugestoes automaticas de documentacao via IA',
      'Exportacao do relatorio de cobertura',
      'Comparacao entre versoes',
    ],
    tips: [
      'Mire em pelo menos 80% de cobertura de documentacao para APIs publicas.',
      'Use as sugestoes de IA como ponto de partida — revise sempre antes de publicar.',
    ],
  },
];

// ---------------------------------------------------------------------------
// Utilitarios
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.DocForge - Manual do Usuario</title>
  <style>
    :root { --primary: #2f6fed; --surface: #ffffff; --bg: #f4f6fa; --text: #1e293b; --text-muted: #64748b; --border: #e2e8f0; --sidebar-bg: #0f172a; --accent: #3b82f6; }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: var(--bg); color: var(--text); line-height: 1.6; }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header { background: var(--sidebar-bg); color: white; padding: 3rem 1.5rem; text-align: center; }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #94a3b8; font-size: 1rem; }
    header .version { color: #64748b; font-size: 0.85rem; margin-top: 0.5rem; }
    nav { background: var(--surface); border-bottom: 1px solid var(--border); padding: 1rem 1.5rem; position: sticky; top: 0; z-index: 100; }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a { display: inline-block; padding: 0.35rem 0.75rem; border-radius: 0.5rem; font-size: 0.85rem; color: var(--text); text-decoration: none; background: var(--bg); transition: background 0.2s; }
    nav a:hover { background: var(--primary); color: white; }
    section { background: var(--surface); border: 1px solid var(--border); border-radius: 1rem; padding: 2rem; margin-bottom: 2rem; }
    section h2 { font-size: 1.5rem; color: var(--primary); margin-bottom: 1rem; padding-bottom: 0.5rem; border-bottom: 2px solid var(--border); }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips { background: var(--bg); border-radius: 0.75rem; padding: 1rem 1.25rem; margin-bottom: 1.25rem; }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #eff6ff; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--accent); }
    .screenshots { display: flex; gap: 1.5rem; margin-top: 1rem; align-items: flex-start; }
    .screenshot-group { position: relative; flex: 1; border: 1px solid var(--border); border-radius: 0.75rem; overflow: hidden; }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge { position: absolute; top: 0.5rem; right: 0.5rem; background: var(--sidebar-bg); color: white; font-size: 0.7rem; padding: 0.2rem 0.5rem; border-radius: 0.35rem; font-weight: 600; text-transform: uppercase; }
    .no-screenshot { background: var(--bg); padding: 2rem; border-radius: 0.75rem; text-align: center; color: var(--text-muted); }
    footer { text-align: center; padding: 2rem 1rem; color: var(--text-muted); font-size: 0.85rem; }
    @media (max-width: 768px) { .screenshots { flex-direction: column; } .screenshot-group.mobile { max-width: 100%; flex: 1; } section { padding: 1.25rem; } }
    @media print { nav { display: none; } section { break-inside: avoid; page-break-inside: avoid; } .screenshots { flex-direction: column; } .screenshot-group.mobile { max-width: 250px; } }
  </style>
</head>
<body>
  <header>
    <h1>RVM.DocForge - Manual do Usuario</h1>
    <p>Documentacao Automatica via Roslyn — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.DocForge</strong> e um sistema de documentacao automatica para projetos .NET.
        Analisa o codigo-fonte com Roslyn e gera documentacao em 7 formatos:
        Markdown, HTML, PDF, Word, JSON, XML e OpenAPI.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>Analise Roslyn</strong> — extrai tipos, metodos, propriedades e XML docs</li>
          <li><strong>7 formatos de saida</strong> — Markdown, HTML, PDF, Word, JSON, XML, OpenAPI</li>
          <li><strong>Geracao automatica</strong> — integra com Git para regenerar em cada commit</li>
          <li><strong>Cobertura de docs</strong> — relatorio de membros sem documentacao</li>
          <li><strong>Sugestoes de IA</strong> — rascunhos automaticos para membros sem XML doc</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Documentacao Automatica</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.DocForge - Manual do Usuario

> Documentacao Automatica via Roslyn — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

---

## Visao Geral

O **RVM.DocForge** gera documentacao automatica para projetos .NET em 7 formatos usando Roslyn.

**Recursos principais:**
- **Analise Roslyn** — extrai tipos, metodos, propriedades e XML docs
- **7 formatos de saida** — Markdown, HTML, PDF, Word, JSON, XML, OpenAPI
- **Geracao automatica** — integra com Git para regenerar em cada commit
- **Cobertura de docs** — relatorio de membros sem documentacao

---

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`));

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) md += `- ${f}\n`;
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) md += `> - ${t}\n`;
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Tecnologia** | ASP.NET Core + Blazor Server |
| **Motor de analise** | Microsoft Roslyn + MSBuild Workspace |
| **Formatos de saida** | Markdown, HTML, PDF, Word, JSON, XML, OpenAPI |
| **Banco de dados** | PostgreSQL 16 |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
